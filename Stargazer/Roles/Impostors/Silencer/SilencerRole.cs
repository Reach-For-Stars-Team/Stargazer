using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Silencer;

public class SilencerRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Silencer";

    public string RoleDescription => "It's so quiet in here.";

    public string RoleLongDescription => "Temporarily silence meetings, reports, and noisemakers.";

    public Color RoleColor => Palette.ImpostorRoleRed;

    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.SilencerRoleIcon
    };
}