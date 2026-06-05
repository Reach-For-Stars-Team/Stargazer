using HarmonyLib;
using Stargazer.Features.Roles;
using UnityEngine;

namespace Stargazer.Patches;

[HarmonyPatch]
public static class RoleManagerPatches
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
    [HarmonyPrefix]
    public static bool AssignRoleOnDeath(RoleManager __instance, ref PlayerControl player)
    {
        GhostRoleAssignment.AssignGhostRoleOnDeath(player);
        return false;
    }

    [HarmonyPatch(typeof(CrewmateRole), nameof(CrewmateRole.RoleIconSolid), MethodType.Getter)]
    public static void CrewmateRole_RoleIconSolid_Getter(CrewmateRole __instance, ref Sprite __result)
    {
        __result = Assets.CrewmateRoleIcon.LoadAsset();
    }
    
    [HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.RoleIconSolid), MethodType.Getter)]
    public static void ImpostorRole_RoleIconSolid_Getter(ImpostorRole __instance, ref Sprite __result)
    {
        __result = Assets.ImpostorRoleIcon.LoadAsset();
    }
}