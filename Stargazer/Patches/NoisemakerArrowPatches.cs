using HarmonyLib;
using Stargazer.Components.Tasks;
using Reactor.Utilities.Extensions;

namespace Stargazer.Patches;

[HarmonyPatch]
public class NoisemakerArrowPatches
{
    [HarmonyPatch(typeof(NoisemakerArrow), nameof(NoisemakerArrow.UpdatePosition))]
    [HarmonyPostfix]
    public static void NoisemakerArrow_UpdatePosition_Postfix(NoisemakerArrow __instance)
    {
        if (PlayerTask.PlayerHasTaskOfType<SilenceTask>(PlayerControl.LocalPlayer))
        {
            __instance.gameObject.Destroy();
        }
    }
}