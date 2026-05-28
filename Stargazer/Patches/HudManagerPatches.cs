using HarmonyLib;
using MiraAPI.LocalSettings;
using Stargazer.Components.Tasks;
using Stargazer.Features;
using Stargazer.Options;
using UnityEngine;

namespace Stargazer.Patches;

[HarmonyPatch]
public class HudManagerPatches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    [HarmonyPostfix]
    public static void HudManager_Start_Postfix(HudManager __instance)
    {
        if (LobbyBehaviour.Instance) Features.InGamePlayerList.CreateButton();
        Camera.main?.gameObject.AddComponent<CameraFXComponent>();
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManager_Update_Postfix(HudManager __instance)
    {
        if (Minigame.Instance == null) return;
        var emergencyMinigame = Minigame.Instance.TryCast<EmergencyMinigame>();
        if (emergencyMinigame == null) return;
        if (PlayerTask.PlayerHasTaskOfType<SilenceTask>(PlayerControl.LocalPlayer))
        {
            emergencyMinigame.StatusText.text = "Shhh!! Be silent!";
            emergencyMinigame.ButtonActive = false;
            emergencyMinigame.ClosedLid.gameObject.SetActive(false);
            emergencyMinigame.ClosedLid.gameObject.SetActive(true);
        }
    }
}