using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Observer;

public class ObserverRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Observer";

    public string RoleDescription => "Keep your eyes peeled.";

    public string RoleLongDescription => "Zoom out to see what others can't.";

    public Color RoleColor => RFSPalette.ObserverRoleColor;

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.ObserverRoleIcon,
    };
}