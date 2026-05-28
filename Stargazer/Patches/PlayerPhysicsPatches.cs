using System;
using HarmonyLib;
using InnerNet;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using Stargazer.Modifiers;
using Stargazer.Roles.Impostors.Mole;
using UnityEngine;

namespace Stargazer.Patches;

[HarmonyPatch]
public class PlayerPhysicsPatches
{
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    [HarmonyPrefix]
    public static bool PlayerPhysics_FixedUpdate_Prefix(PlayerPhysics __instance)
    {
        if (__instance == null) return true;
        if (__instance.myPlayer == null) return true;
        if (__instance.myPlayer.GetModifierComponent() == null) return true;
        bool handleTunnelingPhysics = __instance.myPlayer.HasModifier<TunnelingModifier>();
        if (handleTunnelingPhysics)
        {
            __instance.myPlayer.GetModifier<TunnelingModifier>()?.PlayerPhysicsFixedUpdate();
        }
        return !handleTunnelingPhysics;
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.UpdateMapImage))]
    [HarmonyPrefix]
    public static bool GameStartManager_UpdateMapImage_Prefix(GameStartManager __instance)
    {
        if (__instance.MapImage == null) return false;
        return true;
    }
}