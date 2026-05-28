using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Phaser;

public class PhaserRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Phaser";

    public string RoleDescription => "Phase through walls.";

    public string RoleLongDescription => "You can phase through walls for an easy get-away.";

    public Color RoleColor => Palette.ImpostorRoleRed;

    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.PhaserRoleIcon
    };
}