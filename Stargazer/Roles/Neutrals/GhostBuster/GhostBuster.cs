using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Stargazer.Utilities;
using Rewired;
using UnityEngine;

namespace Stargazer.Roles.Neutrals.GhostBuster;

public class GhostBusterRole : CrewmateRole, INeutralRole
{
    public string RoleName => "Ghost Buster";

    public string RoleDescription => "Catch them ghosts";

    public string RoleLongDescription => "Use traps, goggles, and all sorts of high tech gear to catch ghosts!";
    
    public Color RoleColor => RFSPalette.GhostBusterColor;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    
    private List<PlayerControl> absorbedPlayers = new();

    public void AddAbsorbedPlayer(PlayerControl player)
    {
        absorbedPlayers.Add(player);
        if (absorbedPlayers.Count >= (int)OptionGroupSingleton<GhostBusterOptions>.Instance.GhostQuota.Value)
        {
            Player.AddModifier<NeutralWinner>();
            if (Player.AmOwner)
            {
                HudManager.Instance.SetHudActive(Player, this, true);
            }
        }
        if (Player.AmOwner) HudManager.Instance.SpawnTextOverlay("+1 Ghost");
    }

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.GhostbusterRoleIcon,
    };
}