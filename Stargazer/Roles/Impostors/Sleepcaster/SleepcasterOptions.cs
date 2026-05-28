using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Sleepcaster;

public class SleepcasterOptions : AbstractOptionGroup<SleepcasterRole>
{
    public override string GroupName => "Silencer Options";

    public ModdedNumberOption AbilityCooldown { get; set; } = new ModdedNumberOption("Ability Cooldown", 45, 30, 75, 15, MiraNumberSuffixes.Seconds);
    
    public ModdedNumberOption AbilityDuration { get; set; } = new ModdedNumberOption("Ability Duration", 20, 15, 30, 5, MiraNumberSuffixes.Seconds);
    
    public ModdedNumberOption AbilityRange { get; set; } = new ModdedNumberOption("Ability Range", 2, 1, 2.5f, 0.5f, MiraNumberSuffixes.Multiplier);
}