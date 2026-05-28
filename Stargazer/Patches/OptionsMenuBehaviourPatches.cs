using HarmonyLib;
using MiraAPI.LocalSettings;
using Stargazer.Features;
using Stargazer.Options;

namespace Stargazer;

[HarmonyPatch]
public class OptionsMenuBehaviourPatches
{
    [HarmonyPatch(typeof(ExitGameButton), nameof(ExitGameButton.OnClick))]
    [HarmonyPrefix]
    public static bool OnExitPrefix(ExitGameButton __instance)
    {
        if (LocalSettingsTabSingleton<ClientOptions>.Instance.ShowLeaveConfirmationPopup.Value)
        {
            ConfirmExitDialog.ShowDialog();
            return false;
        }
        else return true;
    }
}