// using System;
// using Il2CppInterop.Runtime.Injection;
// using Stargazer.Mapping;
// using Stargazer.Utilities;
// using UnityEngine;

// namespace Stargazer.DebugTools;

// public class Rope2DBehaviour : MonoBehaviour
// {
//     static Rope2DBehaviour()
//     {
//         ClassInjector.RegisterTypeInIl2Cpp<Rope2DBehaviour>();
//     }

//     public Rope2DBehaviour(IntPtr ptr) : base(ptr)
//     {
//     }

//     public PlayerControl FirstPlayer;
//     public PlayerControl SecondPlayer;

//     public Transform First;
//     public Transform Second;

//     public Vector2 FirstOffset = Vector2.zero;
//     public Vector2 SecondOffset = Vector2.zero;

//     public int SegmentCount = 16;
//     public float TotalLength = 5f;
//     public int SolverIterations = 4;

//     public float PointFriction = 0.92f;

//     public float EndpointCorrectionStrength = 0.35f;
//     public float MaxEndpointCorrectionPerFrame = 0.035f;
//     public float EndpointSlack = 0.6f;
//     public float EndpointWeightInSegments = 0f;

//     public bool UseMapCollision = true;
//     public float CollisionStep = 0.08f;

//     public bool DestroyIfPlayerInvalid = true;

//     public bool TeleportWhenTooFar = true;
//     public float HardTeleportDistance = 12f;
//     public float TeleportNearOtherDistance = 1.2f;

//     public bool FixStuckSegments = true;
//     public float StuckSearchRadius = 0.7f;
//     public int StuckSearchSteps = 16;

//     public bool MovePlayersFromInvalidTiles = true;
//     public float InvalidTileCheckInterval = 0.15f;
//     public float InvalidTileMoveSpeed = 4.5f;
//     public float InvalidTileStopDistance = 0.06f;

//     private Vector2[] points;
//     private Vector2[] previousPoints;

//     private LineRenderer line;
//     private float segmentLength;

//     private float nextInvalidTileCheckTime;

//     private bool localRescueActive;
//     private PlayerControl localRescuePlayer;
//     private Vector2 localRescueTarget;

//     private Collider2D[] localRescueColliders;
//     private bool[] localRescueColliderStates;

//     public static Rope2DBehaviour Create(
//         PlayerControl first,
//         PlayerControl second,
//         float totalLength = 5f,
//         int segmentCount = 16,
//         Color? color = null,
//         float width = 0.08f)
//     {
//         if (!first || !second)
//             return null;

//         GameObject obj = new("Rope2DBehaviour");

//         var rope = obj.AddComponent<Rope2DBehaviour>();

//         rope.FirstPlayer = first;
//         rope.SecondPlayer = second;

//         rope.First = first.transform;
//         rope.Second = second.transform;

//         rope.TotalLength = totalLength;
//         rope.SegmentCount = Mathf.Max(2, segmentCount);

//         rope.Init(color ?? Color.white, width);

//         return rope;
//     }

//     private void Init(Color color, float width)
//     {
//         SegmentCount = Mathf.Max(2, SegmentCount);
//         segmentLength = TotalLength / SegmentCount;

//         int pointCount = SegmentCount + 1;

//         points = new Vector2[pointCount];
//         previousPoints = new Vector2[pointCount];

//         Vector2 firstPos = GetFirstAnchor();
//         Vector2 secondPos = GetSecondAnchor();

//         for (int i = 0; i < pointCount; i++)
//         {
//             float t = i / (float)(pointCount - 1);
//             Vector2 pos = Vector2.Lerp(firstPos, secondPos, t);

//             points[i] = pos;
//             previousPoints[i] = pos;
//         }

//         GameObject lineObj = new("RopeLine");
//         lineObj.transform.SetParent(transform, false);

//         line = lineObj.AddComponent<LineRenderer>();
//         line.useWorldSpace = true;
//         line.positionCount = pointCount;
//         line.startWidth = width;
//         line.endWidth = width;
//         line.startColor = color;
//         line.endColor = color;
//         line.material = new Material(Shader.Find("Sprites/Default"));
//         line.sortingOrder = 0;
//     }

//     private void Update()
//     {
//         if (!First || !Second || ShouldDestroyRope())
//         {
//             Destroy(gameObject);
//             return;
//         }

//         if (MovePlayersFromInvalidTiles)
//         {
//             UpdateInvalidTileRescue();
//         }

//         if (TeleportWhenTooFar && TryHardTeleportIfTooFar())
//             return;

//         if (points == null || previousPoints == null)
//         {
//             Init(Color.white, 0.08f);
//         }

//         points[0] = GetFirstAnchor();
//         points[points.Length - 1] = GetSecondAnchor();

//         SimulateMiddlePoints();

//         for (int i = 0; i < SolverIterations; i++)
//         {
//             SolveDistanceConstraints();
//             CollideMiddlePoints();

//             if (FixStuckSegments)
//                 FixStuckMiddlePoints();
//         }

//         ApplyEndpointCorrections();
//         DrawRope();
//     }

//     private bool ShouldDestroyRope()
//     {
//         if (!DestroyIfPlayerInvalid)
//             return false;

//         return IsPlayerInvalid(FirstPlayer) || IsPlayerInvalid(SecondPlayer);
//     }

//     private bool IsPlayerInvalid(PlayerControl player)
//     {
//         return !player ||
//                player.Data == null ||
//                player.Data.IsDead ||
//                player.Data.Disconnected;
//     }

//     private bool IsPlayerValid(PlayerControl player)
//     {
//         return !IsPlayerInvalid(player);
//     }

//     private Vector2 GetFirstAnchor()
//     {
//         return (Vector2)First.position + FirstOffset;
//     }

//     private Vector2 GetSecondAnchor()
//     {
//         return (Vector2)Second.position + SecondOffset;
//     }

//     private void SimulateMiddlePoints()
//     {
//         for (int i = 1; i < points.Length - 1; i++)
//         {
//             Vector2 current = points[i];
//             Vector2 velocity = (points[i] - previousPoints[i]) * PointFriction;

//             previousPoints[i] = current;

//             TryMovePoint(i, current + velocity);
//         }
//     }

//     private void SolveDistanceConstraints()
//     {
//         for (int i = 0; i < points.Length - 1; i++)
//         {
//             SolveSegment(i, i + 1);
//         }
//     }

//     private void SolveSegment(int a, int b)
//     {
//         Vector2 delta = points[b] - points[a];
//         float distance = delta.magnitude;

//         if (distance <= 0.001f)
//             return;

//         float excess = distance - segmentLength;

//         if (excess <= 0f)
//             return;

//         bool aEndpoint = a == 0 || a == points.Length - 1;
//         bool bEndpoint = b == 0 || b == points.Length - 1;

//         float aWeight = aEndpoint ? EndpointWeightInSegments : 1f;
//         float bWeight = bEndpoint ? EndpointWeightInSegments : 1f;

//         float totalWeight = aWeight + bWeight;
//         if (totalWeight <= 0.001f)
//             return;

//         Vector2 correction = delta.normalized * excess;

//         if (aWeight > 0f)
//             TryMovePoint(a, points[a] + correction * (aWeight / totalWeight));

//         if (bWeight > 0f)
//             TryMovePoint(b, points[b] - correction * (bWeight / totalWeight));
//     }

//     private void ApplyEndpointCorrections()
//     {
//         float ropeLength = GetCurrentRopeLength();

//         float allowedLength = TotalLength + EndpointSlack;
//         float excess = ropeLength - allowedLength;

//         if (excess <= 0f)
//             return;

//         Vector2 firstAnchor = GetFirstAnchor();
//         Vector2 secondAnchor = GetSecondAnchor();

//         Vector2 firstPullTarget = points.Length > 1
//             ? points[1] - FirstOffset
//             : secondAnchor - FirstOffset;

//         Vector2 secondPullTarget = points.Length > 1
//             ? points[points.Length - 2] - SecondOffset
//             : firstAnchor - SecondOffset;

//         MoveTransformSoftlyByExcess(First, firstPullTarget, excess);
//         MoveTransformSoftlyByExcess(Second, secondPullTarget, excess);
//     }

//     private float GetCurrentRopeLength()
//     {
//         float length = 0f;

//         for (int i = 0; i < points.Length - 1; i++)
//         {
//             length += Vector2.Distance(points[i], points[i + 1]);
//         }

//         return length;
//     }

//     private void MoveTransformSoftlyByExcess(Transform target, Vector2 wantedPosition, float excess)
//     {
//         Vector2 current = target.position;
//         Vector2 delta = wantedPosition - current;

//         if (delta.sqrMagnitude <= 0.000001f)
//             return;

//         float maxByExcess = excess * 0.5f * EndpointCorrectionStrength;
//         float maxCorrection = Mathf.Min(MaxEndpointCorrectionPerFrame, maxByExcess);

//         Vector2 correction = Vector2.ClampMagnitude(delta, maxCorrection);
//         Vector2 next = current + correction;

//         if (UseMapCollision && !IsSegmentWalkable(current, next))
//             return;

//         SetTransformPosition(target, next);
//     }

//     private void TryMovePoint(int index, Vector2 next)
//     {
//         Vector2 current = points[index];

//         if (!UseMapCollision)
//         {
//             points[index] = next;
//             return;
//         }

//         if (IsSegmentWalkable(current, next))
//         {
//             points[index] = next;
//             return;
//         }

//         Vector2 xOnly = new(next.x, current.y);
//         if (IsSegmentWalkable(current, xOnly))
//         {
//             points[index] = xOnly;
//             return;
//         }

//         Vector2 yOnly = new(current.x, next.y);
//         if (IsSegmentWalkable(current, yOnly))
//         {
//             points[index] = yOnly;
//             return;
//         }

//         points[index] = current;
//     }

//     private void CollideMiddlePoints()
//     {
//         if (!UseMapCollision)
//             return;

//         for (int i = 1; i < points.Length - 1; i++)
//         {
//             if (!IsWalkable(points[i]))
//             {
//                 points[i] = previousPoints[i];
//             }
//         }
//     }

//     private void FixStuckMiddlePoints()
//     {
//         if (!UseMapCollision)
//             return;

//         for (int i = 1; i < points.Length - 1; i++)
//         {
//             if (IsWalkable(points[i]))
//                 continue;

//             if (TryFindNearestWalkable(points[i], out Vector2 fixedPos))
//             {
//                 points[i] = fixedPos;
//                 previousPoints[i] = fixedPos;
//                 continue;
//             }

//             PlayerControl nearestPlayer = GetNearestWalkableRopePlayer(points[i]);

//             if (nearestPlayer)
//             {
//                 Vector2 nearPlayer = FindNearestWalkablePositionNearPoint(
//                     nearestPlayer.GetTruePosition(),
//                     points[i]
//                 );

//                 if (IsWalkable(nearPlayer))
//                 {
//                     points[i] = nearPlayer;
//                     previousPoints[i] = nearPlayer;
//                 }
//             }
//         }
//     }

//     private bool TryHardTeleportIfTooFar()
//     {
//         if (!FirstPlayer || !SecondPlayer)
//             return false;

//         Vector2 firstPos = FirstPlayer.GetTruePosition();
//         Vector2 secondPos = SecondPlayer.GetTruePosition();

//         float distance = Vector2.Distance(firstPos, secondPos);

//         if (distance <= HardTeleportDistance)
//             return false;

//         if (FirstPlayer.AmOwner)
//         {
//             TeleportPlayerNear(FirstPlayer, SecondPlayer);
//             ResetRopePoints();
//             return true;
//         }

//         if (SecondPlayer.AmOwner)
//         {
//             TeleportPlayerNear(SecondPlayer, FirstPlayer);
//             ResetRopePoints();
//             return true;
//         }

//         return false;
//     }

//     private void TeleportPlayerNear(PlayerControl playerToTeleport, PlayerControl targetPlayer)
//     {
//         if (!playerToTeleport || !targetPlayer)
//             return;

//         Vector2 targetPos = targetPlayer.GetTruePosition();
//         Vector2 selfPos = playerToTeleport.GetTruePosition();

//         Vector2 direction = selfPos - targetPos;

//         if (direction.sqrMagnitude <= 0.001f)
//             direction = Vector2.down;

//         direction.Normalize();

//         Vector2 wantedPos = targetPos + direction * TeleportNearOtherDistance;
//         Vector2 teleportPos = FindNearestWalkablePositionNearPoint(targetPos, wantedPos);

//         if (!IsWalkable(teleportPos))
//             teleportPos = FindNearestWalkablePosition(targetPos);

//         Debug.Log("[Rope2DBehaviour] Hard teleporting " +
//                   playerToTeleport.Data.PlayerName +
//                   " near " +
//                   targetPlayer.Data.PlayerName +
//                   " to " +
//                   teleportPos);

//         if (playerToTeleport.NetTransform)
//         {
//             playerToTeleport.NetTransform.SnapTo(teleportPos);
//         }
//         else
//         {
//             SetTransformPosition(playerToTeleport.transform, teleportPos);
//         }
//     }

//     private void UpdateInvalidTileRescue()
//     {
//         if (localRescueActive)
//         {
//             ContinueInvalidTileRescue();
//             return;
//         }

//         if (Time.time < nextInvalidTileCheckTime)
//             return;

//         nextInvalidTileCheckTime = Time.time + InvalidTileCheckInterval;

//         TryStartInvalidTileRescue(FirstPlayer, SecondPlayer);
//         TryStartInvalidTileRescue(SecondPlayer, FirstPlayer);
//     }

//     private void TryStartInvalidTileRescue(PlayerControl player, PlayerControl other)
//     {
//         if (localRescueActive)
//             return;

//         if (!player || !player.AmOwner)
//             return;

//         if (IsPlayerInvalid(player))
//             return;

//         Vector2 playerPos = player.GetTruePosition();

//         if (IsWalkable(playerPos))
//             return;

//         Vector2 targetPos;

//         if (IsPlayerValid(other) && IsWalkable(other.GetTruePosition()))
//         {
//             targetPos = FindNearestWalkablePositionNearPoint(
//                 other.GetTruePosition(),
//                 playerPos
//             );
//         }
//         else
//         {
//             targetPos = FindNearestWalkablePosition(playerPos);
//         }

//         if (!IsWalkable(targetPos))
//             return;

//         localRescuePlayer = player;
//         localRescueTarget = targetPos;
//         localRescueActive = true;

//         SetLocalPlayerCollision(player, false);

//         Debug.Log("[Rope2DBehaviour] Started invalid tile rescue for " +
//                   player.Data.PlayerName +
//                   " to " +
//                   targetPos);
//     }

//     private void ContinueInvalidTileRescue()
//     {
//         if (!localRescuePlayer || IsPlayerInvalid(localRescuePlayer))
//         {
//             FinishInvalidTileRescue();
//             return;
//         }

//         Vector2 current = localRescuePlayer.transform.position;

//         Vector2 next = Vector2.MoveTowards(
//             current,
//             localRescueTarget,
//             InvalidTileMoveSpeed * Time.deltaTime
//         );

//         SetTransformPosition(localRescuePlayer.transform, next);

//         if (Vector2.Distance(next, localRescueTarget) <= InvalidTileStopDistance)
//         {
//             SetTransformPosition(localRescuePlayer.transform, localRescueTarget);

//             if (localRescuePlayer.NetTransform)
//             {
//                 localRescuePlayer.NetTransform.SnapTo(localRescueTarget);
//             }

//             FinishInvalidTileRescue();
//         }
//     }

//     private void FinishInvalidTileRescue()
//     {
//         if (localRescuePlayer)
//         {
//             SetLocalPlayerCollision(localRescuePlayer, true);
//         }

//         localRescueActive = false;
//         localRescuePlayer = null;

//         ResetRopePoints();

//         Debug.Log("[Rope2DBehaviour] Finished invalid tile rescue.");
//     }

//     private void SetLocalPlayerCollision(PlayerControl player, bool enabled)
//     {
//         if (!player || !player.AmOwner)
//             return;

//         if (!enabled)
//         {
//             localRescueColliders = player.GetComponentsInChildren<Collider2D>();
//             localRescueColliderStates = new bool[localRescueColliders.Length];

//             for (int i = 0; i < localRescueColliders.Length; i++)
//             {
//                 if (!localRescueColliders[i])
//                     continue;

//                 localRescueColliderStates[i] = localRescueColliders[i].enabled;
//                 localRescueColliders[i].enabled = false;
//             }

//             if (player.Collider)
//                 player.Collider.enabled = false;

//             return;
//         }

//         if (localRescueColliders != null && localRescueColliderStates != null)
//         {
//             for (int i = 0; i < localRescueColliders.Length; i++)
//             {
//                 if (!localRescueColliders[i])
//                     continue;

//                 bool oldState = i < localRescueColliderStates.Length && localRescueColliderStates[i];
//                 localRescueColliders[i].enabled = oldState;
//             }
//         }
//         else
//         {
//             foreach (var col in player.GetComponentsInChildren<Collider2D>())
//             {
//                 if (col)
//                     col.enabled = true;
//             }

//             if (player.Collider)
//                 player.Collider.enabled = true;
//         }

//         localRescueColliders = null;
//         localRescueColliderStates = null;
//     }

//     private void ResetRopePoints()
//     {
//         if (points == null || previousPoints == null)
//             return;

//         Vector2 firstPos = GetFirstAnchor();
//         Vector2 secondPos = GetSecondAnchor();

//         for (int i = 0; i < points.Length; i++)
//         {
//             float t = i / (float)(points.Length - 1);
//             Vector2 pos = Vector2.Lerp(firstPos, secondPos, t);

//             points[i] = pos;
//             previousPoints[i] = pos;
//         }
//     }

//     private PlayerControl GetNearestWalkableRopePlayer(Vector2 from)
//     {
//         PlayerControl best = null;
//         float bestDistance = float.MaxValue;

//         TryCheckNearestPlayer(FirstPlayer, from, ref best, ref bestDistance);
//         TryCheckNearestPlayer(SecondPlayer, from, ref best, ref bestDistance);

//         return best;
//     }

//     private void TryCheckNearestPlayer(PlayerControl player, Vector2 from, ref PlayerControl best, ref float bestDistance)
//     {
//         if (!IsPlayerValid(player))
//             return;

//         Vector2 pos = player.GetTruePosition();

//         if (!IsWalkable(pos))
//             return;

//         float distance = Vector2.Distance(from, pos);

//         if (distance < bestDistance)
//         {
//             bestDistance = distance;
//             best = player;
//         }
//     }

//     private Vector2 FindNearestWalkablePosition(Vector2 position)
//     {
//         if (IsWalkable(position))
//             return position;

//         if (TryFindNearestWalkable(position, out Vector2 result))
//             return result;

//         PlayerControl nearestPlayer = GetNearestWalkableRopePlayer(position);

//         if (nearestPlayer)
//             return nearestPlayer.GetTruePosition();

//         return position;
//     }

//     private Vector2 FindNearestWalkablePositionNearPoint(Vector2 center, Vector2 preferredPosition)
//     {
//         if (IsWalkable(preferredPosition))
//             return preferredPosition;

//         float bestDistance = float.MaxValue;
//         Vector2 bestPosition = center;
//         bool found = false;

//         int rings = 12;
//         int steps = Mathf.Max(8, StuckSearchSteps);

//         for (int ring = 1; ring <= rings; ring++)
//         {
//             float radius = 0.25f * ring;

//             for (int i = 0; i < steps; i++)
//             {
//                 float angle = i / (float)steps * Mathf.PI * 2f;

//                 Vector2 candidate = center + new Vector2(
//                     Mathf.Cos(angle),
//                     Mathf.Sin(angle)
//                 ) * radius;

//                 if (!IsWalkable(candidate))
//                     continue;

//                 float distance = Vector2.Distance(candidate, preferredPosition);

//                 if (distance < bestDistance)
//                 {
//                     bestDistance = distance;
//                     bestPosition = candidate;
//                     found = true;
//                 }
//             }

//             if (found)
//                 return bestPosition;
//         }

//         return FindNearestWalkablePosition(center);
//     }

//     private bool TryFindNearestWalkable(Vector2 position, out Vector2 result)
//     {
//         int rings = 14;
//         int steps = Mathf.Max(8, StuckSearchSteps);

//         for (int ring = 1; ring <= rings; ring++)
//         {
//             float radius = StuckSearchRadius * ring / 6f;

//             for (int i = 0; i < steps; i++)
//             {
//                 float angle = i / (float)steps * Mathf.PI * 2f;

//                 Vector2 candidate = position + new Vector2(
//                     Mathf.Cos(angle),
//                     Mathf.Sin(angle)
//                 ) * radius;

//                 if (IsWalkable(candidate))
//                 {
//                     result = candidate;
//                     return true;
//                 }
//             }
//         }

//         result = position;
//         return false;
//     }

//     private bool IsSegmentWalkable(Vector2 from, Vector2 to)
//     {
//         float distance = Vector2.Distance(from, to);
//         int steps = Mathf.Max(1, Mathf.CeilToInt(distance / CollisionStep));

//         for (int i = 0; i <= steps; i++)
//         {
//             float t = i / (float)steps;
//             Vector2 point = Vector2.Lerp(from, to, t);

//             if (!IsWalkable(point))
//                 return false;
//         }

//         return true;
//     }

//     private bool IsWalkable(Vector2 position)
//     {
//         return RandomizationUtils.IsWalkable(position);
//     }

//     private void SetTransformPosition(Transform target, Vector2 position)
//     {
//         target.position = new Vector3(
//             position.x,
//             position.y,
//             position.y / 1000f
//         );
//     }

//     private void DrawRope()
//     {
//         if (!line)
//             return;

//         line.positionCount = points.Length;

//         for (int i = 0; i < points.Length; i++)
//         {
//             Vector2 p = points[i];

//             line.SetPosition(i, new Vector3(
//                 p.x,
//                 p.y,
//                 p.y / 1000f + 0.0020f
//             ));
//         }
//     }

//     private void OnDestroy()
//     {
//         if (localRescueActive && localRescuePlayer)
//         {
//             SetLocalPlayerCollision(localRescuePlayer, true);
//         }

//         if (line)
//         {
//             UnityObject.Destroy(line.gameObject);
//         }
//     }
// }