using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Crewmates.Paranoiac;

public class ParanoiacOptions : AbstractOptionGroup<ParanoiacRole>
{
    public override string GroupName { get; } = "Paranoiac Settings";
    
    public ModdedNumberOption AbilityCooldown =>
        new("Ability Cooldown", 30f, 15f, 60f, 15f, MiraNumberSuffixes.Seconds);
    
    public ModdedNumberOption AbilityDuration =>
        new("Ability Duration", 15f, 10f, 20f, 2.5f, MiraNumberSuffixes.Seconds);
}