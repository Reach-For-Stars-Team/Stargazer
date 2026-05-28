using System.Collections.Generic;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Mole;


public class MoleRole : ImpostorRole, ICustomRole
{
    public override bool IsAffectedByComms => false;
    public List<Vent> MinedVents { get; set; } = new();
    public string RoleName => "Mole";
    public string RoleDescription => "Place vents around the map";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor => Palette.ImpostorRoleRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = true,
        CanGetKilled = true,
        CanUseVent = true,
        Icon = Assets.MoleRoleIcon,
        TasksCountForProgress = false,
        IntroSound = Assets.DigSfx
    };

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        // remove existing task header.
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return GameManager.Instance.DidImpostorsWin(gameOverReason);
    }

    public override void OnMeetingStart()
    {
        CustomButtonSingleton<Dig>.Instance.SetUses(1);
    }
    
    [RegisterEvent]
    public static void CanUseEvent(PlayerCanUseEvent e)
    {
        if (e.IsVent && PlayerControl.LocalPlayer.HasModifier<TunnelingModifier>()) e.Cancel();
    }
}