using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using Stargazer.Mapping;
using UnityEngine;

namespace Stargazer.Mapping;

[HarmonyPatch]
public static class IntroPathMapBlocker
{
    private static bool Loading;
    private static bool Ready;
    private static bool SpawnedDebugFollower;

    private const float CellSize = 0.20f;
    private const int WorkPerFrame = 400;

    public static MethodBase GetStateMachineMoveNext<T>(string methodName)
    {
        var typeName = typeof(T).FullName;
        var showRoleStateMachine =
            typeof(T)
                .GetNestedTypes()
                .FirstOrDefault(x=>x.Name.Contains(methodName));

        if (showRoleStateMachine == null)
        {
            Debug.LogError($"Failed to find {methodName} state machine for {typeName}");
            return null;
        }

        var moveNext = AccessTools.Method(showRoleStateMachine, "MoveNext");
        if (moveNext == null)
        {
            Debug.LogError($"Failed to find MoveNext method for {typeName}.{methodName}");
            return null;
        }

        Debug.Log($"Found {methodName}.MoveNext");
        return moveNext;
    }
    public static MethodBase TargetMethod()
    {
        return GetStateMachineMoveNext<IntroCutscene>(nameof(IntroCutscene.CoBegin))!;
    }

    public static bool Prefix(Il2CppObjectBase __instance, ref bool __result)
    {
        if (Ready)
            return true;

        if (!Loading)
        {
            EnsureLoaded();

            if (!Loading && !Ready)
            {
                Ready = true;
                return true;
            }
        }

        __result = true;
        return false;
    }

    private static IEnumerator CoLoadMapBeforeIntro()
    {
        bool loadedFromFile = false;
        bool shouldGenerate = false;

        try
        {
            string mapName = PatchMapFile.GetCurrentMapName();
            Debug.Log("[IntroPathMapBlocker] Current map detected: " + mapName);

            if (PatchMapFile.TryLoadCurrentMap(out var loadedMap, out var source))
            {
                Debug.Log("[IntroPathMapBlocker] Map file found. Source=" + source);
                Debug.Log("[IntroPathMapBlocker] Loading cached path data. Map=" + loadedMap.MapName +
                          ", Nodes=" + loadedMap.Nodes.Count +
                          ", Links=" + loadedMap.Links.Count +
                          ", DebugCells=" + loadedMap.DebugCells.Count);

                RandomizationUtils.LoadCachedPathMapFileData(loadedMap);

                Debug.Log("[IntroPathMapBlocker] Cached path data loaded successfully.");

                loadedFromFile = true;
            }
            else
            {
                Debug.LogWarning("[IntroPathMapBlocker] Map file not found. Will generate path map.");
                shouldGenerate = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[IntroPathMapBlocker] Exception while loading path map: " + e);
            shouldGenerate = true;
        }

        if (loadedFromFile)
        {
            Ready = true;
            SpawnDebugFollowerAfterMapLoad();
            Debug.Log("[IntroPathMapBlocker] Ready=true after file load. Intro can start now.");
            yield break;
        }

        if (shouldGenerate)
        {
            Debug.Log("[IntroPathMapBlocker] Generating path map...");

            yield return RandomizationUtils.CoMapPathfindingCells(
                CellSize,
                _ => { },
                WorkPerFrame
            );

            Debug.Log("[IntroPathMapBlocker] Path map generated.");
        }

        Ready = true;
        SpawnDebugFollowerAfterMapLoad();
        Debug.Log("[IntroPathMapBlocker] Ready=true at end. Intro can start now.");
    }

    private static void SpawnDebugFollowerAfterMapLoad()
    {
        if (SpawnedDebugFollower)
            return;

        if (!PlayerControl.LocalPlayer)
        {
            Debug.LogWarning("[IntroPathMapBlocker] Could not spawn debug follower, LocalPlayer is null.");
            return;
        }

        SpawnedDebugFollower = true;

        Debug.Log("[IntroPathMapBlocker] Spawning debug follower after path map load.");

        // PathToPlayerDebug.CreateFollower(
        //     startPosition: PlayerControl.LocalPlayer.GetTruePosition(),
        //     targetGetter: () => PlayerControl.LocalPlayer.GetTruePosition(),
        //     sprite: Assets.SnailSprite.LoadAsset(),
        //     scale: 0.55f,
        //     color: Color.white,
        //     speed: 0.9f
        // );
    }

    public static void Reset()
    {
        Debug.Log("[IntroPathMapBlocker] Reset called.");

        Loading = false;
        Ready = false;
        SpawnedDebugFollower = false;

        RandomizationUtils.ClearCachedPathfinding();
        PathToPlayerDebug.ClearAllFollowers();
    }

    public static void ReloadForNewGame()
    {
        Debug.Log("[IntroPathMapBlocker] ReloadForNewGame called.");

        Loading = false;
        Ready = false;
        SpawnedDebugFollower = false;

        RandomizationUtils.ClearCachedPathfinding();
        PathToPlayerDebug.ClearAllFollowers();

        EnsureLoaded();
    }

    public static void EnsureLoaded()
    {
        if (Ready || Loading)
            return;

        if (!ShipStatus.Instance)
            return;

        Loading = true;

        Debug.Log("[IntroPathMapBlocker] EnsureLoaded called.");

        if (HudManager.Instance)
        {
            HudManager.Instance.StartCoroutine(CoLoadMapBeforeIntro());
        }
        else if (PlayerControl.LocalPlayer)
        {
            PlayerControl.LocalPlayer.StartCoroutine(CoLoadMapBeforeIntro());
        }
        else if (ShipStatus.Instance)
        {
            ShipStatus.Instance.StartCoroutine(CoLoadMapBeforeIntro());
        }
        else
        {
            Debug.LogWarning("[IntroPathMapBlocker] Could not start map loading coroutine.");
            Ready = true;
        }
    }
    public static void ReloadForFreeplay()
    {
        Debug.Log("[IntroPathMapBlocker] ReloadForFreeplay called.");
        ReloadForNewGame();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class ResetIntroPathMapBlockerPatch
{
    public static void Prefix()
    {
        IntroPathMapBlocker.Reset();
    }
}

[HarmonyPatch]
public class PathMapShipStartPatch
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    [HarmonyPostfix]
    public static void ShipStatusStartPostfix(ShipStatus __instance)
    {
        IntroPathMapBlocker.ReloadForNewGame();
    }
}
