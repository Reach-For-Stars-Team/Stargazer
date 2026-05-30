using System.Collections.Generic;
using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Neutrals.Roleless;

public class RolelessRole : CrewmateRole, INeutralRole
{
    public string RoleName => "Roleless";

    public string RoleDescription => "Switch roles, before it's too late.";

    public string RoleLongDescription => "You're roleless, swap roles with someone else to make it their problem!";

    public Color RoleColor => Palette.DisabledGrey;
    
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.PyromaniacRoleIcon
    };
}