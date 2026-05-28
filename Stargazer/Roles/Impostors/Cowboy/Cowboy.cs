using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Cowboy;

public class CowboyRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Cowboy";

    public string RoleDescription => "Lasso nearby players.";

    public string RoleLongDescription => "Lasso the nearest player to pull them to your location.";

    public Color RoleColor => Palette.ImpostorRoleRed;

    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.CowboyRoleIcon,
    };
}