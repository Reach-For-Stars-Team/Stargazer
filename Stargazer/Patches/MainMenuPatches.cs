using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Stargazer.Features.MainMenu;
using UnityEngine;

namespace Stargazer.Patches;

[HarmonyPatch]
public class MainMenuPatches
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Awake))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void MainMenuManager_Awake_Postfix(MainMenuManager __instance)
    {
        RFSLogo.Create(__instance);
        ReworkedMainMenu.SetUp(__instance);
    }
}