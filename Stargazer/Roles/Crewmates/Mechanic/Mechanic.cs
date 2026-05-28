using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Mechanic;

public class MechanicRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Mechanic";
    public string RoleDescription => "Mess with the vents!";
    public string RoleLongDescription => "Unlink vent systems at will.";
    public Color RoleColor => RFSPalette.MechanicRoleColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.MechanicRoleIcon
    };
}