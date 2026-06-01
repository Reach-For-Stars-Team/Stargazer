using AchievementsAPI.API;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using Stargazer.Roles.Impostors.Cowboy;
using Stargazer.Roles.Impostors.Sleepcaster;
using UnityEngine;

namespace Stargazer.Features;

public class StargazerAchievements : AchievementsTab
{
    public override string Name => "Stargazer Achievements";
    public override Color GetTabColor()
    {
        return RFSPalette.RfsColor2;
    }

    public override Sprite GetIcon()
    {
        return Assets.ModIcon.LoadAsset();
    }

    public BaseAchievement MechanicAchievement1 { get; } = new BaseAchievement("You're not going anywhere",
        "Disconnect a vent direction.", "Stargazer.Resources.RoleIcons.mechanic.png");
    public BaseAchievement MechanicAchievement2 { get; } = new BaseAchievement("Gonna sit there?",
        "Disconnect a vent from all paths.", "Stargazer.Resources.RoleIcons.mechanic.png");
    
    public BaseAchievement CarrierAchievement1 { get; } = new("Weight lifter",
        "Carry and throw a body.",  "Stargazer.Resources.RoleIcons.carrier.png");
    public BaseAchievement CarrierAchievement2 { get; } = new("Hidden in plain sight",
        "Throw a body into a vent.", "Stargazer.Resources.RoleIcons.carrier.png");
    public BaseAchievement CarrierAchievement3 { get; } =
        new("Ouchies!", "Throw a body onto someone to crush them to death.", "Stargazer.Resources.RoleIcons.carrier.png");
    
    public BaseAchievement CowboyAchievement1 { get; } = new("Snatch em'", "Pull a player to you.",  "Stargazer.Resources.RoleIcons.cowboy.png");
    public BaseAchievement CowboyAchievement2 { get; } = new("On the hunt.", "Use the lasso ability 3 times in one game.", "Stargazer.Resources.RoleIcons.cowboy.png");
    public BaseAchievement CowboyAchievement3 { get; } = new("You're mine!", "Pull a player that was put to sleep.", "Stargazer.Resources.RoleIcons.cowboy.png");

    public BaseAchievement SleepcasterAchievement1 { get; } =
        new BaseAchievement("Zzz", "Put atleast two players to sleep with one ability usage.", "Stargazer.Resources.RoleIcons.sleepcaster.png");
    public BaseAchievement SleepcasterAchievement2 { get; } = new("Sleep paralysis", "Kill an asleep player.", "Stargazer.Resources.RoleIcons.sleepcaster.png");
}