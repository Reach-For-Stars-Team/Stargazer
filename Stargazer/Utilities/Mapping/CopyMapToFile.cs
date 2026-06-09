// using System.Collections;
// using System.Collections.Generic;
// using BepInEx.Unity.IL2CPP.Utils;
// using HarmonyLib;
// using Stargazer.Mapping;
// using Stargazer.Utilities;
// using UnityEngine;

// namespace Stargazer.Mapping;

// [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
// public static class CopyMapToFile
// {
//     private const float CellSize = 0.20f;
//     private const int WorkPerFrame = 250;

//     private static Coroutine Routine;

//     public static void Postfix()
//     {
//         if (!HudManager.Instance) return;
//         if (!ShipStatus.Instance) return;
//         if (!PlayerControl.LocalPlayer) return;

//         if (!Input.GetKeyDown(KeyCode.F6))
//             return;

//         if (Routine != null)
//         {
//             HudManager.Instance.SpawnTextOverlay("Map save already running");
//             return;
//         }

//         Routine = HudManager.Instance.StartCoroutine(CoSaveMap());
//     }

//     private static IEnumerator CoSaveMap()
//     {
//         string mapName = PatchMapFile.GetCurrentMapName();

//         HudManager.Instance.SpawnTextOverlay("Saving path map: " + mapName);

//         yield return Mapping.CoMapPathfindingCells(
//             CellSize,
//             _ => { },
//             WorkPerFrame
//         );

//         var data = Mapping.CreateCachedPathMapFileData(
//             mapName,
//             CellSize,
//             new List<Mapping.SpawnDebugCell>()
//         );

//         PatchMapFile.SaveCurrentMap(data);

//         HudManager.Instance.SpawnTextOverlay("Saved path map to C:/Map/" + mapName + ".map");

//         Routine = null;
//     }
// }