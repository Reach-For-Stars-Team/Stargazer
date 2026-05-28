using System.Linq;
using HarmonyLib;
using Il2CppSystem;
using MiraAPI.Modifiers;
using Stargazer.Components;
using Stargazer.Modifiers;
using Stargazer.Roles.Crewmates.Lifesaver;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Patches;

[HarmonyPatch]
public class PlayerControlPatches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPriority(99)]
    [HarmonyPostfix]
    public static void PlayerControl_FixedUpdate_Postfix(PlayerControl __instance)
    {
        if (__instance == null) return;
        if (__instance.HasModifier<InvisibleModifier>()) __instance.Visible = false;
        
        if (PlayerControl.LocalPlayer.HasModifier<CanSeeGhostsModifier>() && __instance.Data.IsDead) __instance.Visible = true;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    [HarmonyPostfix]
    public static void PlayerControl_Start_Postfix(PlayerControl __instance)
    {
        __instance.StartCoroutine(Effects.ActionAfterDelay(0.01f, new System.Action((() =>
        {
            var iconObj = new GameObject("RoleIcon");
            iconObj.transform.SetParent(__instance.cosmetics.nameTextContainer.transform);
            iconObj.transform.localPosition =
                new((__instance.cosmetics.nameText.GetRenderedWidth(true) + 0.2f) * -1f, 0);
            var roleIconBehaviour = iconObj.AddComponent<RoleIconBehaviour>();
            roleIconBehaviour.myPlayer = __instance;
            roleIconBehaviour.myRenderer = iconObj.AddComponent<SpriteRenderer>();
        }))));
    }
    
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Revive))]
    [HarmonyPostfix]
    public static void PlayerControl_Revive_Postfix(PlayerControl __instance)
    {
        foreach (var body in UnityObject.FindObjectsOfType<DeadBody>().Where(x => x.ParentId == __instance.PlayerId))
        {
            body.gameObject.Destroy();
            __instance.StartCoroutine(__instance.CoSetRole(__instance.Data.RoleWhenAlive.Value, true));
            __instance.AddModifier<RevivedModifier>();
        }
    }
}