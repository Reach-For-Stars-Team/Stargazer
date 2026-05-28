using System.Collections.Generic;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Florist;

[MiraIgnore]
public class FloristRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Florist";

    public string RoleDescription => "throw new System.NotImplementedException()";

    public string RoleLongDescription => "throw new System.NotImplementedException()";

    public Color RoleColor => Palette.ImpostorRoleRed;

    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this);

    public enum FlowerTypes
    {
        TallGrass,
        Flowers,
        Thorns
    }

    public static List<Color> FlowerColors = new([new (1f, 0f, 0.8f), 
        new (0.8f, 0.6f, 1f), 
        new (0f, 0.8f, 0.8f), 
        new (1f, 0.1f, 0.1f), 
        new (1f, 0.8f, 0f), 
        new (0.25f, 0.8f, 0.25f)]);
    
}