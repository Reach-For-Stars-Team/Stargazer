using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Cowboy;

public class CowboyOptions : AbstractOptionGroup<CowboyRole>
{
    public override string GroupName => "Cowboy Options";

    public ModdedNumberOption AbilityCooldown { get; set; } = new ModdedNumberOption("Ability Cooldown", 45, 30, 75, 15, MiraNumberSuffixes.Seconds);
    
    public ModdedNumberOption PullDuration { get; set; } = new ModdedNumberOption("Pull Duration", 2.5f, 1, 5, 0.5f, MiraNumberSuffixes.Seconds);
    
    public ModdedNumberOption WrangledPlayerSpeedMultiplier { get; set; } = new ModdedNumberOption("Wrangled Player Speed Multiplier", 1.5f, 0.5f, 3f, 0.25f, MiraNumberSuffixes.Multiplier);
}