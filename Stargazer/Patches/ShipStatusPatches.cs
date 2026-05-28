using HarmonyLib;
using MiraAPI.LocalSettings;
using Stargazer.Features.Freeplay;
using Stargazer.Options;

namespace Stargazer.Patches;

[HarmonyPatch]
public class ShipStatusPatches
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    [HarmonyPostfix]
    public static void ShipStatusStartPostfix(ShipStatus __instance)
    {
        if (!DestroyableSingleton<TutorialManager>.InstanceExists)
            return;
        FreeplayOptionsLaptop.Create();
    }
}