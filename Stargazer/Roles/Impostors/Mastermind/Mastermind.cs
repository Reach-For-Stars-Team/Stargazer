using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using UnityEngine;


namespace Stargazer.Roles.Impostors.Mastermind;
public class MastermindRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Mastermind";
    public string RoleDescription => "Convert people to your team!";
    public string RoleLongDescription => "Play rock paper scissors with someone: if you win, they join you. If you lose, they'll know your role.";
    public Color RoleColor => Palette.ImpostorRoleRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.CarrierRoleIcon //TEMPORARY!! CHANGE WHEN ICON EXISTS
    };

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        // remove existing task header. thanks pix totally didnt steal ur code
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return GameManager.Instance.DidImpostorsWin(gameOverReason);
    }
}