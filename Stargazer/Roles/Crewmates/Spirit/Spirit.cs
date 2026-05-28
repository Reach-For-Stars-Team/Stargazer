using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Spirit;

public class SpiritRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Spirit";
    
    public string RoleDescription => "Exit your body to collect intel.";
    
    public string RoleLongDescription => "Exit your body to watch people as a ghost.";
    
    public Color RoleColor => RFSPalette.SpiritRoleColor;
    
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.SpiritRoleIcon
    }; 
}