using MiraAPI.Hud;
using MiraAPI.Roles;
using PowerTools;
using Stargazer.Components;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Paranoiac;

public class ParanoiacRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Paranoiac";
    public string RoleLongDescription => "Be vigilant of other players!";
    public string RoleDescription => RoleLongDescription;
    public Color RoleColor => RFSPalette.ParanoiacRoleColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.ParanoiacRoleIcon,
        ShowInFreeplay = true,
        HideSettings = false
    };
}