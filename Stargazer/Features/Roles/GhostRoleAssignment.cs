using System.Linq;
using AmongUs.GameOptions;
using Il2CppSystem;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;

namespace Stargazer.Features.Roles;

public static class GhostRoleAssignment
{
    public static RoleTypes[] RolePool { get; set; } = [];

    public static void AssignGhostRoleOnDeath(PlayerControl player)
    {
        if (player.Data.Role.IsImpostor) AssignImpostorGhostRole(player);
        else if (!player.Data.Role.IsImpostor) AssignCrewmateGhostRole(player);
    }

    public static void AssignCrewmateGhostRole(PlayerControl player)
    {
        var Role = RolePool.Random();
        var chance = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(Role);
        if (Helpers.CheckChance(chance)) player.RpcSetRole(Role);
        else player.RpcSetRole(RoleTypes.CrewmateGhost);
    }

    public static void AssignImpostorGhostRole(PlayerControl player)
    {
        player.RpcSetRole(RoleTypes.ImpostorGhost, true);
        //Will expand once there's impostor ghost roles
    }

    [RegisterEvent]
    public static void OnGameStart(IntroBeginEvent e)
    {
        foreach (var r in RoleManager.Instance.AllRoles.ToArray().Where(x => x.IsDead))
        {
            var i = 0;
            while (i != GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(r.Role))
            {
                RolePool = RolePool.Append(r.Role).ToArray();
                i++;
            }
        }
    }
}