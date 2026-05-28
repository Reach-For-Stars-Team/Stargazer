using MiraAPI.Modifiers;
using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Neutrals;

public class NeutralGhost : CrewmateGhostRole, ICustomRole
{

    public string RoleName => "Neutral Ghost";
    public string RoleDescription => "you are dead! watch the rest of the game unfold!";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor => Color.gray;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = false,
        CanGetKilled = true,
        CanUseVent = false,
        CanUseSabotage = false,
        TasksCountForProgress = false,
        RoleHintType = RoleHintType.TaskHint
    };

    public virtual bool CanLocalPlayerSeeRole(PlayerControl player)
    {
        return false;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return Player.HasModifier<NeutralWinner>();
    }

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (Player.HasModifier<NeutralWinner>())
            PlayerTask.GetOrCreateTask<ImportantTextTask>(Player).Text =
                "You have <b>won!</b> Sit back and watch the game unfold!";
        else if (!Player.HasModifier<NeutralWinner>())
            PlayerTask.GetOrCreateTask<ImportantTextTask>(Player).Text =
                "You have <b>lost!</b> watch the rest of the game unfold!";
    }
}