using HarmonyLib;
using Stargazer.Features.Roles;

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
}