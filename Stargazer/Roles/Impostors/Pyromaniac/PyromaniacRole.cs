using System.Collections.Generic;
using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Pyromaniac;

public class PyromaniacRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Pyromaniac";

    public string RoleDescription => "Watch the world burn.";

    public string RoleLongDescription => "Douse and Light players to achieve chained kills!";

    public Color RoleColor => Palette.ImpostorRoleRed;

    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public List<PlayerControl> DousedPlayers = new();
    
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.PyromaniacRoleIcon
    };
}