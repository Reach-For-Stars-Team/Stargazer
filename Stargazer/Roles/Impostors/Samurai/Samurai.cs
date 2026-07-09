using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Samurai;

public class SamuraiRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Samurai";
    public string RoleDescription => "Slash people with your katana!";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor => Palette.ImpostorRoleRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = true,
        CanGetKilled = true,
        CanUseVent = true,
        Icon = Assets.PlaceHolder,
        TasksCountForProgress = false,
        IntroSound = Assets.DigSfx
    };
    [RegisterEvent]
    public static void CanUseEvent(PlayerCanUseEvent e)
    {
        if (e.IsVent && PlayerControl.LocalPlayer.HasModifier<KatanaModifier>()) e.Cancel();
    }
}