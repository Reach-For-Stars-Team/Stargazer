using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Actor;

public class ActorRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Actor";
    public string RoleDescription => "Befriend players to know their alignment!";
    public string RoleLongDescription => "You can befriend players by acting to slowly find out their alignment!";
    public Color RoleColor => RFSPalette.ActorRoleColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.ActorRoleIcon
    };
}