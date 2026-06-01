using AchievementsAPI.API;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Stargazer.Features;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Sleepcaster;

public class SleepcasterRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Sleepcaster";

    public string RoleDescription => "Pacify players to put them to sleep.";

    public string RoleLongDescription => "Pacify players to put them to sleep temporarily.";

    public Color RoleColor => Palette.ImpostorRoleRed;

    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.SleepcasterRoleIcon
    };

    [RegisterEvent]
    public static void OnMurder(BeforeMurderEvent e)
    {
        if (e.Source.AmOwner && e.Target.HasModifier<SleepyModifier>()) AchievementsTabSingleton<StargazerAchievements>.Instance.SleepcasterAchievement2.Unlock();
    }
}