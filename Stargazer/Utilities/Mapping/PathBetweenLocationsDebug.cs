using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Hazel;
using Reactor.Utilities.Extensions;
using Stargazer.Networking;
using Stargazer.Utilities;
using UnityEngine;

namespace Stargazer.Mapping;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class PathToPlayerDebug
{
    public enum FollowerVisibility
    {
        Local,
        Everyone
    }

    public enum FollowerObjectType : byte
    {
        SpriteOnly = 0,
        DeadBody = 1,
        Player = 2,
        GameObjectName = 3
    }

    public enum FollowerTargetType : byte
    {
        LocalPlayer = 0,
        Player = 1,
        Position = 2
    }

    public enum NetworkFollowerAction : byte
    {
        CreateOrUpdate = 0,
        Delete = 1,
        StopFollowing = 2,
        SyncPath = 3
    }

    private class NetworkFollowerState
    {
        public GameObject Object;
        public bool DestroyOnStop;
    }

    private class PathFollowerInstance
    {
        public string Id;
        public byte OwnerId = byte.MaxValue;
        public string ObjectKey;

        public Vector2? StartLocation;
        public Vector2 MovingPosition;

        public Vector2 RemoteAuthoritativePosition;
        public bool HasRemoteAuthoritativePosition;
        public int LastAppliedPathHash;
        public int LastSentPathHash;
        public float LastPathSyncTime;

        public GameObject Root;
        public SpriteRenderer StartCircle;
        public SpriteRenderer TargetCircle;
        public LineRenderer PathLine;

        public GameObject FollowerObject;
        public SpriteRenderer FollowerRenderer;
        public bool UsingCustomFollowerObject;

        public List<Vector2> CurrentPath = new();
        public int CurrentPathIndex;

        public Vector2 CurrentMoveTarget;
        public bool HasCurrentMoveTarget;

        public List<Vector2> PendingPath;
        public Vector2 PendingPathStart;
        public bool HasPendingPath;

        public Vector2 LastTargetPathPos;
        public Vector2 LastMovingPathPos;
        public bool HasLastPathPositions;

        public float NextRepathTime;
        public bool ShowedNoMapWarning;

        public Coroutine PathRoutine;

        public float Speed = DefaultDebugDotSpeed;
        public float Scale = CircleScale;
        public Color Color = Blue;
        public Sprite Sprite;
        public Vector2 ObjectOffset;

        public bool StopAtTarget;

        public System.Func<Vector2> TargetGetter;
    }

    private static readonly Dictionary<string, NetworkFollowerState> NetworkFollowers = new();
    private static readonly Dictionary<string, PathFollowerInstance> Followers = new();
    private static readonly Dictionary<string, Coroutine> FollowerSyncCoroutines = new();
    private static readonly Dictionary<string, string> ObjectFollowerIds = new();
    private static readonly List<PathFollowerInstance> FollowerSnapshot = new();

    private const float RepathInterval = 0.30f;
    private const float MinMoveToRepath = 0.28f;

    private const float DefaultDebugDotSpeed = 0.9f;
    private const float ReachPointDistance = 0.05f;

    private const float SpecialLinkSnapDistance = 0.75f;
    private const float RemoteHardCorrectionDistance = 4.0f;
    private const float RemoteSoftCorrectionDistance = 0.12f;
    private const float RemoteCorrectionStrength = 0.60f;
    private const float RemoteCatchupMultiplier = 1.35f;

    private const int PathWorkPerFrame = 45;

    private const float CircleScale = 0.55f;
    private const float TargetCircleScale = 0.45f;
    private const float LineWidth = 0.12f;
    private const float Z = -31f;

    private const float NetworkSyncInterval = 0.55f;
    private const float PathSyncMinInterval = 0.90f;
    private const float StopAtTargetDistance = 0.12f;

    private static readonly Color Blue = new(0f, 0.35f, 1f, 1f);

    private static int NextLocalFollowerId;

    public static void Postfix()
    {
        if (!HudManager.Instance) return;
        if (!ShipStatus.Instance) return;
        if (!PlayerControl.LocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            DeadBody body = UnityEngine.Object.FindObjectsOfType<DeadBody>()
                .OrderBy(b => Vector2.Distance(
                    b.transform.position,
                    PlayerControl.LocalPlayer.GetTruePosition()
                ))
                .FirstOrDefault();

            if (!body)
            {
                HudManager.Instance.SpawnTextOverlay("No dead body found");
                return;
            }

            string followerId = "body_follower_" + body.ParentId;

            StartNetworkedFollower(
                followerId: followerId,
                startPosition: body.transform.position,
                targetType: FollowerTargetType.Player,
                targetId: PlayerControl.LocalPlayer.PlayerId,
                targetPosition: PlayerControl.LocalPlayer.GetTruePosition(),
                objectType: FollowerObjectType.DeadBody,
                objectId: body.ParentId,
                objectName: "",
                scale: 1f,
                color: Color.white,
                speed: 2.25f,
                objectOffset: Vector2.zero
            );
        }

        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            DeadBody body = UnityEngine.Object.FindObjectsOfType<DeadBody>()
                .OrderBy(b => Vector2.Distance(
                    b.transform.position,
                    PlayerControl.LocalPlayer.GetTruePosition()
                ))
                .FirstOrDefault();

            if (!body)
            {
                HudManager.Instance.SpawnTextOverlay("No dead body found");
                return;
            }

            string followerId = "body_follower_" + body.ParentId;
            RpcStopFollowing(followerId);
        }

        // Test GoTo: najbliższe ciało idzie do miejsca, w którym stoisz przy kliknięciu.
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            DeadBody body = UnityEngine.Object.FindObjectsOfType<DeadBody>()
                .OrderBy(b => Vector2.Distance(
                    b.transform.position,
                    PlayerControl.LocalPlayer.GetTruePosition()
                ))
                .FirstOrDefault();

            if (!body)
            {
                HudManager.Instance.SpawnTextOverlay("No dead body found");
                return;
            }

            string followerId = "body_goto_" + body.ParentId;

            StartNetworkedGoTo(
                followerId: followerId,
                startPosition: body.transform.position,
                destination: PlayerControl.LocalPlayer.GetTruePosition(),
                objectType: FollowerObjectType.DeadBody,
                objectId: body.ParentId,
                objectName: "",
                scale: 1f,
                color: Color.white,
                speed: 2.25f,
                objectOffset: Vector2.zero
            );

            HudManager.Instance.SpawnTextOverlay("Body GoTo set");
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            ClearAllFollowers();
        }

        if (PathDebugVisibility.TogglePressedThisFrame())
        {
            FollowerSnapshot.Clear();
            foreach (var follower in Followers.Values)
                FollowerSnapshot.Add(follower);

            foreach (var follower in FollowerSnapshot)
                ApplyVisibility(follower);

            FollowerSnapshot.Clear();

            HudManager.Instance.SpawnTextOverlay(PathDebugVisibility.Visible ? "Show path debug" : "Hide path debug");
        }

        FollowerSnapshot.Clear();
        foreach (var follower in Followers.Values)
            FollowerSnapshot.Add(follower);

        foreach (var follower in FollowerSnapshot)
        {
            if (follower == null) continue;
            if (!follower.Root) continue;
            if (!follower.StartLocation.HasValue) continue;

            UpdateMovingStartDot(follower);

            // Tylko właściciel liczy pathfinding. Inni odtwarzają ścieżkę z SyncPath.
            if (!IsLocalPathOwner(follower))
                continue;

            if (Time.time < follower.NextRepathTime)
                continue;

            follower.NextRepathTime = Time.time + RepathInterval;
            TryStartPathUpdate(follower);
        }

        FollowerSnapshot.Clear();
    }

    public static void CreateFollower(
        Vector2 startPosition,
        System.Func<Vector2> targetGetter,
        Sprite sprite = null,
        GameObject followObject = null,
        float scale = CircleScale,
        Color? color = null,
        float speed = DefaultDebugDotSpeed,
        Vector2? objectOffset = null)
    {
        string followerId = "local_follower_" + NextLocalFollowerId++;

        var follower = CreateFollowerInstance(
            followerId,
            byte.MaxValue,
            "",
            startPosition,
            targetGetter,
            sprite,
            followObject,
            scale,
            color,
            speed,
            objectOffset,
            false
        );

        Followers[followerId] = follower;
    }

    public static void StartNetworkedFollower(
        string followerId,
        Vector2 startPosition,
        FollowerTargetType targetType,
        byte targetId,
        Vector2 targetPosition,
        FollowerObjectType objectType,
        byte objectId,
        string objectName,
        float scale,
        Color color,
        float speed,
        Vector2 objectOffset)
    {
        StartNetworkedFollowerInternal(
            followerId,
            startPosition,
            targetType,
            targetId,
            targetPosition,
            objectType,
            objectId,
            objectName,
            scale,
            color,
            speed,
            objectOffset,
            false
        );
    }

    public static void StartNetworkedGoTo(
        string followerId,
        Vector2 startPosition,
        Vector2 destination,
        FollowerObjectType objectType,
        byte objectId,
        string objectName,
        float scale,
        Color color,
        float speed,
        Vector2 objectOffset)
    {
        StartNetworkedFollowerInternal(
            followerId,
            startPosition,
            FollowerTargetType.Position,
            byte.MaxValue,
            destination,
            objectType,
            objectId,
            objectName,
            scale,
            color,
            speed,
            objectOffset,
            true
        );
    }

    private static void StartNetworkedFollowerInternal(
        string followerId,
        Vector2 startPosition,
        FollowerTargetType targetType,
        byte targetId,
        Vector2 targetPosition,
        FollowerObjectType objectType,
        byte objectId,
        string objectName,
        float scale,
        Color color,
        float speed,
        Vector2 objectOffset,
        bool stopAtTarget)
    {
        RpcCreateOrUpdateFollower(
            followerId,
            startPosition,
            targetType,
            targetId,
            targetPosition,
            objectType,
            objectId,
            objectName,
            scale,
            color,
            speed,
            objectOffset,
            stopAtTarget
        );

        StartFollowerSync(
            followerId,
            targetType,
            targetId,
            targetPosition,
            objectType,
            objectId,
            objectName,
            scale,
            color,
            speed,
            objectOffset,
            stopAtTarget
        );
    }

    public static void RpcCreateOrUpdateFollower(
        string followerId,
        Vector2 startPosition,
        FollowerTargetType targetType,
        byte targetId,
        Vector2 targetPosition,
        FollowerObjectType objectType,
        byte objectId,
        string objectName,
        float scale,
        Color color,
        float speed,
        Vector2 objectOffset,
        bool stopAtTarget = false)
    {
        if (!PlayerControl.LocalPlayer) return;

        byte ownerId = PlayerControl.LocalPlayer.PlayerId;

        PlayerControl.LocalPlayer.RpcCreateOrUpdatePathFollower(
            followerId,
            ownerId,
            startPosition.x,
            startPosition.y,
            (byte)targetType,
            targetId,
            targetPosition.x,
            targetPosition.y,
            (byte)objectType,
            objectId,
            objectName ?? "",
            scale,
            color.r,
            color.g,
            color.b,
            color.a,
            speed,
            objectOffset.x,
            objectOffset.y,
            stopAtTarget
        );

        ReceiveCreateOrUpdateFollower(
            followerId,
            ownerId,
            startPosition,
            targetType,
            targetId,
            targetPosition,
            objectType,
            objectId,
            objectName,
            scale,
            color,
            speed,
            objectOffset,
            stopAtTarget
        );
    }

    private static void RpcSyncFollowerPath(
        string followerId,
        byte ownerId,
        Vector2 ownerPosition,
        List<Vector2> path)
    {
        if (!PlayerControl.LocalPlayer) return;
        if (path == null || path.Count < 2) return;

        float[] pathData = new float[path.Count * 2];

        for (int i = 0; i < path.Count; i++)
        {
            pathData[i * 2] = path[i].x;
            pathData[i * 2 + 1] = path[i].y;
        }

        PlayerControl.LocalPlayer.RpcSyncFollowerPath(
            followerId,
            ownerId,
            ownerPosition.x,
            ownerPosition.y,
            pathData
        );
    }

    public static void RpcDeleteFollower(string followerId)
    {
        if (PlayerControl.LocalPlayer)
        {
            PlayerControl.LocalPlayer.RpcDeletePathFollower(followerId);
        }

        DeleteFollower(followerId);
    }

    public static void RpcStopFollowing(string followerId)
    {
        if (PlayerControl.LocalPlayer)
        {
            PlayerControl.LocalPlayer.RpcStopFollowingPathFollower(followerId);
        }

        StopFollowing(followerId);
    }

    public static void ReceiveCreateOrUpdateFollower(
        string followerId,
        byte ownerId,
        Vector2 startPosition,
        FollowerTargetType targetType,
        byte targetId,
        Vector2 targetPosition,
        FollowerObjectType objectType,
        byte objectId,
        string objectName,
        float scale,
        Color color,
        float speed,
        Vector2 objectOffset,
        bool stopAtTarget)
    {
        GameObject followObject = ResolveFollowerObject(objectType, objectId, objectName);
        string objectKey = GetFollowerObjectKey(objectType, objectId, objectName);

        ClaimFollowerObject(objectKey, followerId);

        if (!Followers.TryGetValue(followerId, out var follower) || follower == null)
        {
            follower = CreateFollowerInstance(
                followerId,
                ownerId,
                objectKey,
                startPosition,
                () => ResolveTargetPosition(targetType, targetId, targetPosition),
                null,
                followObject,
                scale,
                color,
                speed,
                objectOffset,
                stopAtTarget
            );

            Followers[followerId] = follower;

            GameObject trackedObject = followObject ? followObject : follower.FollowerObject;

            if (trackedObject)
            {
                NetworkFollowers[followerId] = new NetworkFollowerState
                {
                    Object = trackedObject,
                    DestroyOnStop = objectType == FollowerObjectType.SpriteOnly
                };
            }

            return;
        }

        bool ownerChanged = follower.OwnerId != ownerId;
        follower.OwnerId = ownerId;
        follower.ObjectKey = objectKey;

        if (ownerChanged)
        {
            StopLocalSync(followerId);

            follower.CurrentPath.Clear();
            follower.CurrentPathIndex = 0;
            follower.HasCurrentMoveTarget = false;

            follower.PendingPath = null;
            follower.HasPendingPath = false;

            follower.HasLastPathPositions = false;
            ForceRepath(follower);
        }

        follower.TargetGetter = () => ResolveTargetPosition(targetType, targetId, targetPosition);
        follower.Scale = scale;
        follower.Color = color;
        follower.Speed = speed;
        follower.ObjectOffset = objectOffset;
        follower.StopAtTarget = stopAtTarget;

        if (!IsLocalPathOwner(follower))
            ReceiveRemoteAuthoritativePosition(follower, startPosition, false);
    }

    public static void ReceiveSyncFollowerPath(
        string followerId,
        byte ownerId,
        Vector2 ownerPosition,
        List<Vector2> path)
    {
        if (!Followers.TryGetValue(followerId, out var follower) || follower == null)
            return;

        if (IsLocalPathOwner(follower))
            return;

        if (follower.OwnerId != ownerId)
            return;

        if (path == null || path.Count < 2)
            return;

        ReceiveRemoteAuthoritativePosition(follower, ownerPosition, false);

        int pathHash = GetPathHash(path);

        if (pathHash == follower.LastAppliedPathHash && follower.HasCurrentMoveTarget)
            return;

        follower.LastAppliedPathHash = pathHash;
        follower.PendingPath = null;
        follower.HasPendingPath = false;
        follower.HasLastPathPositions = false;

        ApplyPath(follower, path);
    }

    public static void DeleteFollower(string followerId)
    {
        StopLocalSync(followerId);

        if (NetworkFollowers.TryGetValue(followerId, out var state))
        {
            if (state.Object && state.DestroyOnStop)
                UnityEngine.Object.Destroy(state.Object);

            NetworkFollowers.Remove(followerId);
        }

        RemoveObjectFollowerOwner(followerId);
        ClearFollower(followerId);
    }

    public static void StopFollowing(string followerId)
    {
        StopLocalSync(followerId);

        if (NetworkFollowers.TryGetValue(followerId, out var state))
        {
            if (state.Object && state.DestroyOnStop)
                UnityEngine.Object.Destroy(state.Object);

            NetworkFollowers.Remove(followerId);
        }

        RemoveObjectFollowerOwner(followerId);
        ClearFollower(followerId);
    }

    private static PathFollowerInstance CreateFollowerInstance(
        string followerId,
        byte ownerId,
        string objectKey,
        Vector2 startPosition,
        System.Func<Vector2> targetGetter,
        Sprite sprite = null,
        GameObject followObject = null,
        float scale = CircleScale,
        Color? color = null,
        float speed = DefaultDebugDotSpeed,
        Vector2? objectOffset = null,
        bool stopAtTarget = false)
    {
        var follower = new PathFollowerInstance
        {
            Id = followerId,
            OwnerId = ownerId,
            ObjectKey = objectKey,
            StartLocation = startPosition,
            MovingPosition = startPosition,
            RemoteAuthoritativePosition = startPosition,
            HasRemoteAuthoritativePosition = true,
            TargetGetter = targetGetter,
            Sprite = sprite,
            Scale = scale,
            Color = color ?? (sprite ? Color.white : Blue),
            Speed = speed,
            ObjectOffset = objectOffset ?? Vector2.zero,
            UsingCustomFollowerObject = followObject,
            StopAtTarget = stopAtTarget
        };

        EnsureVisuals(follower);

        if (followObject)
            SetFollowerObject(follower, followObject, scale);

        ForceRepath(follower);
        SetFollowerPosition(follower, follower.MovingPosition);

        return follower;
    }

    private static void StartFollowerSync(
        string followerId,
        FollowerTargetType targetType,
        byte targetId,
        Vector2 targetPosition,
        FollowerObjectType objectType,
        byte objectId,
        string objectName,
        float scale,
        Color color,
        float speed,
        Vector2 objectOffset,
        bool stopAtTarget)
    {
        if (HudManager.Instance == null)
            return;

        StopLocalSync(followerId);

        FollowerSyncCoroutines[followerId] = HudManager.Instance.StartCoroutine(CoSyncFollower(
            followerId,
            targetType,
            targetId,
            targetPosition,
            objectType,
            objectId,
            objectName,
            scale,
            color,
            speed,
            objectOffset,
            stopAtTarget
        ));
    }

    private static IEnumerator CoSyncFollower(
        string followerId,
        FollowerTargetType targetType,
        byte targetId,
        Vector2 targetPosition,
        FollowerObjectType objectType,
        byte objectId,
        string objectName,
        float scale,
        Color color,
        float speed,
        Vector2 objectOffset,
        bool stopAtTarget)
    {
        while (true)
        {
            if (!Followers.TryGetValue(followerId, out var follower) || follower == null)
            {
                FollowerSyncCoroutines.Remove(followerId);
                yield break;
            }

            if (!PlayerControl.LocalPlayer || follower.OwnerId != PlayerControl.LocalPlayer.PlayerId)
            {
                FollowerSyncCoroutines.Remove(followerId);
                yield break;
            }

            GameObject obj = ResolveFollowerObject(objectType, objectId, objectName);

            if (!obj && objectType != FollowerObjectType.SpriteOnly)
            {
                RpcStopFollowing(followerId);
                FollowerSyncCoroutines.Remove(followerId);
                yield break;
            }

            RpcCreateOrUpdateFollower(
                followerId,
                follower.MovingPosition,
                targetType,
                targetId,
                targetPosition,
                objectType,
                objectId,
                objectName,
                scale,
                color,
                speed,
                objectOffset,
                stopAtTarget
            );

            yield return new WaitForSeconds(NetworkSyncInterval);
        }
    }

    private static bool IsLocalPathOwner(PathFollowerInstance follower)
    {
        if (follower == null)
            return false;

        if (follower.OwnerId == byte.MaxValue)
            return true;

        return PlayerControl.LocalPlayer && follower.OwnerId == PlayerControl.LocalPlayer.PlayerId;
    }
    private static void ReceiveRemoteAuthoritativePosition(PathFollowerInstance follower, Vector2 ownerPosition, bool force)
    {
        if (follower == null)
            return;

        follower.RemoteAuthoritativePosition = ownerPosition;
        follower.HasRemoteAuthoritativePosition = true;

        float dist = Vector2.Distance(follower.MovingPosition, ownerPosition);

        // Twardy snap tylko gdy naprawdę się rozjechało. Wcześniej było za agresywne,
        // przez co u innych wyglądało jak stop -> teleport.
        if (force || dist > RemoteHardCorrectionDistance)
        {
            follower.MovingPosition = ownerPosition;
            follower.HasCurrentMoveTarget = false;
            follower.CurrentPath.Clear();
            follower.CurrentPathIndex = 0;
            follower.PendingPath = null;
            follower.HasPendingPath = false;
            SetFollowerPosition(follower, follower.MovingPosition);
        }
    }

    private static void ApplyRemoteSoftCorrection(PathFollowerInstance follower)
    {
        if (follower == null)
            return;

        if (IsLocalPathOwner(follower))
            return;

        if (!follower.HasRemoteAuthoritativePosition)
            return;

        float dist = Vector2.Distance(follower.MovingPosition, follower.RemoteAuthoritativePosition);

        if (dist <= RemoteSoftCorrectionDistance)
            return;

        if (dist > RemoteHardCorrectionDistance)
        {
            follower.MovingPosition = follower.RemoteAuthoritativePosition;
            follower.HasCurrentMoveTarget = false;
            follower.CurrentPath.Clear();
            follower.CurrentPathIndex = 0;
            SetFollowerPosition(follower, follower.MovingPosition);
            return;
        }

        // Gdy remote dalej ma path, korekta jest lekka i nie psuje ścieżki.
        // Gdy remote nie ma już punktu ruchu, nie stoi w miejscu, tylko dogania ostatnią pozycję ownera.
        if (follower.HasCurrentMoveTarget)
        {
            follower.MovingPosition = Vector2.Lerp(
                follower.MovingPosition,
                follower.RemoteAuthoritativePosition,
                RemoteCorrectionStrength * Time.deltaTime
            );
        }
        else
        {
            float catchupSpeed = Mathf.Max(follower.Speed * RemoteCatchupMultiplier, dist * 4f);
            follower.MovingPosition = Vector2.MoveTowards(
                follower.MovingPosition,
                follower.RemoteAuthoritativePosition,
                catchupSpeed * Time.deltaTime
            );
        }
    }

    private static int GetPathHash(List<Vector2> path)
    {
        if (path == null)
            return 0;

        unchecked
        {
            int hash = 17;
            hash = hash * 31 + path.Count;

            for (int i = 0; i < path.Count; i++)
            {
                hash = hash * 31 + Mathf.RoundToInt(path[i].x * 100f);
                hash = hash * 31 + Mathf.RoundToInt(path[i].y * 100f);
            }

            return hash;
        }
    }

    private static void StopLocalSync(string followerId)
    {
        if (HudManager.Instance && FollowerSyncCoroutines.TryGetValue(followerId, out var co) && co != null)
            HudManager.Instance.StopCoroutine(co);

        FollowerSyncCoroutines.Remove(followerId);
    }

    private static string GetFollowerObjectKey(
        FollowerObjectType objectType,
        byte objectId,
        string objectName)
    {
        switch (objectType)
        {
            case FollowerObjectType.DeadBody:
                return "DeadBody:" + objectId;

            case FollowerObjectType.Player:
                return "Player:" + objectId;

            case FollowerObjectType.GameObjectName:
                return "GameObjectName:" + (objectName ?? "");

            case FollowerObjectType.SpriteOnly:
            default:
                return "";
        }
    }

    private static void ClaimFollowerObject(string objectKey, string newFollowerId)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            return;

        if (ObjectFollowerIds.TryGetValue(objectKey, out var oldFollowerId))
        {
            if (!string.IsNullOrWhiteSpace(oldFollowerId) && oldFollowerId != newFollowerId)
                StopFollowing(oldFollowerId);
        }

        ObjectFollowerIds[objectKey] = newFollowerId;
    }

    private static void RemoveObjectFollowerOwner(string followerId)
    {
        if (!Followers.TryGetValue(followerId, out var follower) || follower == null)
            return;

        if (string.IsNullOrWhiteSpace(follower.ObjectKey))
            return;

        if (ObjectFollowerIds.TryGetValue(follower.ObjectKey, out var currentFollowerId) && currentFollowerId == followerId)
            ObjectFollowerIds.Remove(follower.ObjectKey);
    }

    private static Vector2 ResolveTargetPosition(
        FollowerTargetType targetType,
        byte targetId,
        Vector2 targetPosition)
    {
        switch (targetType)
        {
            case FollowerTargetType.LocalPlayer:
                return PlayerControl.LocalPlayer
                    ? PlayerControl.LocalPlayer.GetTruePosition()
                    : targetPosition;

            case FollowerTargetType.Player:
            {
                var player = PlayerControlUtils.GetPlayerById(targetId);
                return player ? player.GetTruePosition() : targetPosition;
            }

            case FollowerTargetType.Position:
            default:
                return targetPosition;
        }
    }

    private static GameObject ResolveFollowerObject(
        FollowerObjectType objectType,
        byte objectId,
        string objectName)
    {
        switch (objectType)
        {
            case FollowerObjectType.DeadBody:
            {
                var body = UnityEngine.Object.FindObjectsOfType<DeadBody>()
                    .FirstOrDefault(b => b && b.ParentId == objectId);

                return body ? body.gameObject : null;
            }

            case FollowerObjectType.Player:
            {
                var player = PlayerControlUtils.GetPlayerById(objectId);
                return player ? player.gameObject : null;
            }

            case FollowerObjectType.GameObjectName:
            {
                if (string.IsNullOrWhiteSpace(objectName))
                    return null;

                return GameObject.Find(objectName);
            }

            case FollowerObjectType.SpriteOnly:
            default:
                return null;
        }
    }

    private static void ForceRepath(PathFollowerInstance follower)
    {
        follower.NextRepathTime = 0f;
    }

    private static void TryStartPathUpdate(PathFollowerInstance follower)
    {
        if (!follower.StartLocation.HasValue)
            return;

        if (follower.PathRoutine != null)
            return;

        if (!RandomizationUtils.HasCachedPathMap)
        {
            SetLine(follower, null);

            if (!follower.ShowedNoMapWarning)
            {
                HudManager.Instance.SpawnTextOverlay("Press F9 first");
                follower.ShowedNoMapWarning = true;
            }

            return;
        }

        follower.ShowedNoMapWarning = false;

        Vector2 targetPos = follower.TargetGetter != null
            ? follower.TargetGetter()
            : PlayerControl.LocalPlayer.GetTruePosition();

        Vector2 pathStart = follower.MovingPosition;

        if (follower.HasLastPathPositions)
        {
            float targetMove = Vector2.Distance(targetPos, follower.LastTargetPathPos);
            float startMove = Vector2.Distance(pathStart, follower.LastMovingPathPos);

            if (targetMove < MinMoveToRepath &&
                startMove < MinMoveToRepath &&
                !IsCurrentMoveBlockedByDoor(follower))
            {
                return;
            }
        }

        follower.LastTargetPathPos = targetPos;
        follower.LastMovingPathPos = pathStart;
        follower.HasLastPathPositions = true;

        SetFollowerPosition(follower, follower.MovingPosition);
        SetCirclePosition(follower.TargetCircle, targetPos);

        follower.PathRoutine = HudManager.Instance.StartCoroutine(CoUpdatePath(follower, pathStart, targetPos));
    }

    private static IEnumerator CoUpdatePath(PathFollowerInstance follower, Vector2 pathStart, Vector2 targetPos)
    {
        bool success = false;
        List<Vector2> resultPath = null;
        Vector2 usedTargetPos = targetPos;

        yield return RandomizationUtils.CoFindPathOnCachedMap(
            pathStart,
            targetPos,
            PathWorkPerFrame,
            (ok, path, finalTargetPos) =>
            {
                success = ok;
                resultPath = path;
                usedTargetPos = finalTargetPos;
            }
        );

        follower.PathRoutine = null;

        if (!follower.Root)
            yield break;

        if (!success || resultPath == null || resultPath.Count < 2 || IsPathRuntimeBlocked(resultPath))
        {
            // Brak drogi / droga przez zamknięte drzwi.
            // Nie przyjmuj nowej ścieżki, ale też nie cofaj obiektu.
            follower.HasCurrentMoveTarget = false;

            follower.PendingPath = null;
            follower.HasPendingPath = false;

            follower.HasLastPathPositions = false;
            follower.NextRepathTime = Time.time + 0.5f;

            SetFollowerPosition(follower, follower.MovingPosition);
            SetCirclePosition(follower.TargetCircle, targetPos);
            SetLine(follower, null);

            TryCompleteStopAtTarget(follower);
            yield break;
        }

        if (IsLocalPathOwner(follower) && follower.OwnerId != byte.MaxValue)
        {
            int pathHash = GetPathHash(resultPath);

            if (pathHash != follower.LastSentPathHash ||
                Time.time - follower.LastPathSyncTime >= PathSyncMinInterval)
            {
                follower.LastSentPathHash = pathHash;
                follower.LastPathSyncTime = Time.time;
                RpcSyncFollowerPath(follower.Id, follower.OwnerId, follower.MovingPosition, resultPath);
            }
        }

        SetCirclePosition(follower.TargetCircle, usedTargetPos);

        ApplyPath(follower, resultPath);
    }
    private static bool IsPathRuntimeBlocked(List<Vector2> path)
    {
        if (path == null || path.Count < 2)
            return false;

        for (int i = 0; i < path.Count - 1; i++)
        {
            if (RandomizationUtils.IsRuntimePathSegmentBlocked(path[i], path[i + 1]))
                return true;
        }

        return false;
    }
    private static void ApplyPath(PathFollowerInstance follower, List<Vector2> path)
    {
        if (path == null || path.Count < 2)
        {
            follower.CurrentPath.Clear();
            follower.CurrentPathIndex = 0;
            follower.HasCurrentMoveTarget = false;
            SetLine(follower, null);
            return;
        }

        int nextIndex = FindBestForwardPathIndex(follower.MovingPosition, path);

        if (nextIndex <= 0 || nextIndex >= path.Count)
        {
            follower.CurrentPath.Clear();
            follower.CurrentPathIndex = 0;
            follower.HasCurrentMoveTarget = false;
            SetLine(follower, null);
            return;
        }

        follower.CurrentPath = path;
        follower.CurrentPathIndex = nextIndex;
        follower.CurrentMoveTarget = follower.CurrentPath[follower.CurrentPathIndex];
        follower.HasCurrentMoveTarget = true;

        SetFollowerPosition(follower, follower.MovingPosition);
        DrawCurrentFollowerLine(follower);
    }

    private static int FindFirstUsefulPathIndex(Vector2 currentPos, List<Vector2> path)
    {
        return FindBestForwardPathIndex(currentPos, path);
    }

    private static int FindBestForwardPathIndex(Vector2 currentPos, List<Vector2> path)
    {
        if (path == null || path.Count < 2)
            return -1;

        int bestIndex = 1;
        float bestDistance = float.MaxValue;
        float bestT = 0f;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 a = path[i - 1];
            Vector2 b = path[i];
            float t;
            Vector2 closest = ClosestPointOnSegment(currentPos, a, b, out t);
            float distance = Vector2.Distance(currentPos, closest);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
                bestT = t;
            }
        }

        if (bestT > 0.82f && bestIndex + 1 < path.Count)
            bestIndex++;

        while (bestIndex < path.Count && Vector2.Distance(currentPos, path[bestIndex]) <= ReachPointDistance * 1.8f)
            bestIndex++;

        if (bestIndex >= path.Count)
            return -1;

        return bestIndex;
    }

    private static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b, out float t)
    {
        Vector2 ab = b - a;
        float len = ab.sqrMagnitude;

        if (len <= 0.00001f)
        {
            t = 0f;
            return a;
        }

        t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / len);
        return a + ab * t;
    }

    private static void UpdateMovingStartDot(PathFollowerInstance follower)
    {
        if (!follower.StartCircle && !follower.FollowerObject)
            return;

        ApplyRemoteSoftCorrection(follower);

        if (!follower.HasCurrentMoveTarget)
        {
            SetFollowerPosition(follower, follower.MovingPosition);
            DrawCurrentFollowerLine(follower);
            TryCompleteStopAtTarget(follower);
            return;
        }

        if (IsLocalPathOwner(follower) &&
            RandomizationUtils.IsRuntimePathSegmentBlocked(follower.MovingPosition, follower.CurrentMoveTarget))
        {
            // Zamknięte drzwi = blokada jak czerwony kwadrat.
            // Nie cofamy, nie snapujemy, nie zmieniamy pozycji.
            follower.HasCurrentMoveTarget = false;

            follower.PendingPath = null;
            follower.HasPendingPath = false;

            follower.HasLastPathPositions = false;
            follower.NextRepathTime = Time.time + 0.5f;

            SetLine(follower, null);
            SetFollowerPosition(follower, follower.MovingPosition);
            return;
        }

        float distanceToTarget = Vector2.Distance(follower.MovingPosition, follower.CurrentMoveTarget);

        if (distanceToTarget > SpecialLinkSnapDistance)
        {
            follower.MovingPosition = follower.CurrentMoveTarget;
            OnReachedCurrentMoveTarget(follower);

            SetFollowerPosition(follower, follower.MovingPosition);
            DrawCurrentFollowerLine(follower);
            return;
        }

        float step = follower.Speed * Time.deltaTime;
        follower.MovingPosition = Vector2.MoveTowards(follower.MovingPosition, follower.CurrentMoveTarget, step);

        if (Vector2.Distance(follower.MovingPosition, follower.CurrentMoveTarget) <= ReachPointDistance)
        {
            follower.MovingPosition = follower.CurrentMoveTarget;
            OnReachedCurrentMoveTarget(follower);
        }

        SetFollowerPosition(follower, follower.MovingPosition);
        DrawCurrentFollowerLine(follower);
    }

    private static void OnReachedCurrentMoveTarget(PathFollowerInstance follower)
    {
        if (follower.HasPendingPath && Vector2.Distance(follower.PendingPathStart, follower.MovingPosition) <= 0.05f)
        {
            var path = follower.PendingPath;

            follower.PendingPath = null;
            follower.HasPendingPath = false;

            follower.HasLastPathPositions = false;
            ForceRepath(follower);

            ApplyPath(follower, path);
            return;
        }

        if (follower.CurrentPath != null && follower.CurrentPath.Count > 1)
        {
            follower.CurrentPathIndex++;

            if (follower.CurrentPathIndex < follower.CurrentPath.Count)
            {
                follower.CurrentMoveTarget = follower.CurrentPath[follower.CurrentPathIndex];
                follower.HasCurrentMoveTarget = true;
            }
            else
            {
                follower.HasCurrentMoveTarget = false;
            }
        }
        else
        {
            follower.HasCurrentMoveTarget = false;
        }

        follower.HasLastPathPositions = false;
        ForceRepath(follower);

        TryCompleteStopAtTarget(follower);
    }

    private static void TryCompleteStopAtTarget(PathFollowerInstance follower)
    {
        if (!follower.StopAtTarget)
            return;

        if (!IsLocalPathOwner(follower))
            return;

        Vector2 targetPos = follower.TargetGetter != null
            ? follower.TargetGetter()
            : follower.MovingPosition;

        if (Vector2.Distance(follower.MovingPosition, targetPos) > StopAtTargetDistance)
            return;

        RpcStopFollowing(follower.Id);
    }

    private static bool IsCurrentMoveBlockedByDoor(PathFollowerInstance follower)
    {
        if (!follower.HasCurrentMoveTarget)
            return false;

        return RandomizationUtils.IsRuntimePathSegmentBlocked(
            follower.MovingPosition,
            follower.CurrentMoveTarget
        );
    }

    private static void DrawCurrentFollowerLine(PathFollowerInstance follower)
    {
        if (!follower.PathLine) return;

        if (!PathDebugVisibility.Visible)
        {
            follower.PathLine.enabled = false;

            if (follower.PathLine.positionCount != 0)
                follower.PathLine.positionCount = 0;

            return;
        }

        if (!follower.HasCurrentMoveTarget || follower.CurrentPath == null || follower.CurrentPath.Count < 2)
        {
            SetLine(follower, null);
            return;
        }

        follower.PathLine.enabled = true;
        int count = 2 + Mathf.Max(0, follower.CurrentPath.Count - follower.CurrentPathIndex - 1);
        follower.PathLine.positionCount = count;

        follower.PathLine.SetPosition(0, new Vector3(follower.MovingPosition.x, follower.MovingPosition.y, Z));
        follower.PathLine.SetPosition(1, new Vector3(follower.CurrentMoveTarget.x, follower.CurrentMoveTarget.y, Z));

        int writeIndex = 2;
        for (int i = follower.CurrentPathIndex + 1; i < follower.CurrentPath.Count; i++)
        {
            Vector2 p = follower.CurrentPath[i];
            follower.PathLine.SetPosition(writeIndex++, new Vector3(p.x, p.y, Z));
        }
    }

    private static void EnsureVisuals(PathFollowerInstance follower)
    {
        if (follower.Root) return;

        follower.Root = new GameObject("PathFollowerRoot_" + follower.Id);

        follower.StartCircle = CreateFollowerRenderer(follower);
        follower.TargetCircle = CreateCircle(follower, "PathTargetCircle", Blue, TargetCircleScale);

        var lineObj = new GameObject("PathLine");
        lineObj.transform.SetParent(follower.Root.transform);

        follower.PathLine = lineObj.AddComponent<LineRenderer>();
        follower.PathLine.useWorldSpace = true;
        follower.PathLine.startWidth = LineWidth;
        follower.PathLine.endWidth = LineWidth;
        follower.PathLine.startColor = Blue;
        follower.PathLine.endColor = Blue;
        follower.PathLine.material = new Material(Shader.Find("Sprites/Default"));
        follower.PathLine.sortingOrder = 1000;
        follower.PathLine.positionCount = 0;

        ApplyVisibility(follower);

        if (follower.StartLocation.HasValue)
        {
            if (follower.MovingPosition == Vector2.zero)
                follower.MovingPosition = follower.StartLocation.Value;

            SetFollowerPosition(follower, follower.MovingPosition);
        }

        if (PlayerControl.LocalPlayer)
            SetCirclePosition(follower.TargetCircle, PlayerControl.LocalPlayer.GetTruePosition());
    }

    private static SpriteRenderer CreateFollowerRenderer(PathFollowerInstance follower)
    {
        var obj = new GameObject("PathFollower");
        obj.transform.SetParent(follower.Root.transform);
        obj.transform.localScale = Vector3.one * follower.Scale;

        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = follower.Sprite ? follower.Sprite : null;
        renderer.color = follower.Color;
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 1000;
        renderer.enabled = true;

        follower.FollowerObject = obj;
        follower.FollowerRenderer = renderer;

        return renderer;
    }

    private static SpriteRenderer CreateCircle(PathFollowerInstance follower, string name, Color color, float scale)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(follower.Root.transform);
        obj.transform.localScale = Vector3.one * scale;

        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = null;
        renderer.color = color;
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 1000;

        return renderer;
    }

    private static void SetFollowerObject(PathFollowerInstance follower, GameObject obj, float scale = 1f)
    {
        if (!obj)
            return;

        follower.FollowerObject = obj;
        follower.FollowerRenderer = follower.FollowerObject.GetComponent<SpriteRenderer>();
        follower.UsingCustomFollowerObject = true;

        follower.FollowerObject.transform.SetParent(null, true);
        follower.FollowerObject.transform.localScale *= scale;

        follower.FollowerObject.transform.position = new Vector3(
            follower.MovingPosition.x,
            follower.MovingPosition.y,
            follower.MovingPosition.y / 1000f
        );

        if (follower.StartCircle)
            follower.StartCircle.enabled = false;
    }

    private static void SetFollowerPosition(PathFollowerInstance follower, Vector2 pos)
    {
        if (follower.UsingCustomFollowerObject && follower.FollowerObject)
        {
            Vector2 finalPos = pos + follower.ObjectOffset;

            follower.FollowerObject.transform.position = new Vector3(
                finalPos.x,
                finalPos.y,
                finalPos.y / 1000f
            );

            return;
        }

        SetCirclePosition(follower.StartCircle, pos);
    }

    private static void SetCirclePosition(SpriteRenderer renderer, Vector2 pos)
    {
        if (!renderer) return;

        renderer.transform.position = new Vector3(pos.x, pos.y, Z);
    }

    private static void SetLine(PathFollowerInstance follower, List<Vector2> path)
    {
        if (!follower.PathLine) return;

        if (!PathDebugVisibility.Visible)
        {
            follower.PathLine.enabled = false;

            if (follower.PathLine.positionCount != 0)
                follower.PathLine.positionCount = 0;

            return;
        }

        follower.PathLine.enabled = true;

        if (path == null || path.Count < 2)
        {
            follower.PathLine.positionCount = 0;
            return;
        }

        follower.PathLine.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
            follower.PathLine.SetPosition(i, new Vector3(path[i].x, path[i].y, Z));
    }

    private static void ApplyVisibility(PathFollowerInstance follower)
    {
        bool visible = PathDebugVisibility.Visible;

        if (follower.StartCircle)
            follower.StartCircle.enabled = !follower.UsingCustomFollowerObject;

        if (follower.FollowerRenderer)
            follower.FollowerRenderer.enabled = true;

        if (follower.TargetCircle)
            follower.TargetCircle.enabled = visible;

        if (follower.PathLine)
            follower.PathLine.enabled = visible;
    }

    private static void ClearFollower(string followerId)
    {
        if (!Followers.TryGetValue(followerId, out var follower))
            return;

        if (follower.PathRoutine != null && HudManager.Instance)
        {
            HudManager.Instance.StopCoroutine(follower.PathRoutine);
            follower.PathRoutine = null;
        }

        if (follower.Root)
            follower.Root.Destroy();

        Followers.Remove(followerId);
    }

    public static void ClearAllFollowers()
    {
        if (HudManager.Instance)
        {
            foreach (var co in FollowerSyncCoroutines.Values)
            {
                if (co != null)
                    HudManager.Instance.StopCoroutine(co);
            }
        }

        FollowerSyncCoroutines.Clear();

        foreach (var followerId in Followers.Keys.ToList())
            ClearFollower(followerId);

        foreach (var pair in NetworkFollowers.ToList())
        {
            var state = pair.Value;

            if (state != null && state.Object && state.DestroyOnStop)
                UnityEngine.Object.Destroy(state.Object);
        }

        NetworkFollowers.Clear();
        ObjectFollowerIds.Clear();
    }
}


public static class PathDebugVisibility
{
    public static bool Visible { get; private set; }

    public static bool TogglePressedThisFrame()
    {
        if (!Input.GetKeyDown(KeyCode.KeypadEnter))
            return false;

        Visible = !Visible;
        return true;
    }
}