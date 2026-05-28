using HarmonyLib;
using Stargazer.Features.Minimap;

namespace Stargazer.Patches;

[HarmonyPatch]
public static class MapBehaviourPatches
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Show))]
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowCountOverlay))]
    [HarmonyPostfix]
    public static void MapBehaviourShowPostfix(MapBehaviour __instance)
    {
        RefreshedMapBehaviour.SetUp(__instance);
    }
}