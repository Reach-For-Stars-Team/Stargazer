using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Carrier;

public class CarrierRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Carrier";
    public string RoleDescription => "Drag Bodies Around the map!";
    public string RoleLongDescription => "Move bodies around to hide them!";
    public Color RoleColor => Palette.ImpostorRoleRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.CarrierRoleIcon
    };
    
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        // remove existing task header.
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return GameManager.Instance.DidImpostorsWin(gameOverReason);
    }

    [RegisterEvent]
    public static void CanUseEvent(PlayerCanUseEvent e)
    {
        if (e.IsVent && PlayerControl.LocalPlayer.HasModifier<CarryingModifier>()) e.Cancel();
    }
}