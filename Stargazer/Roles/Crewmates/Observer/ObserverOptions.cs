using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Crewmates.Observer;

public class ObserverOptions : AbstractOptionGroup<ObserverRole>
{
    public override string GroupName => "Observer Options";

    public ModdedNumberOption AbilityCooldown { get; } =
        new ModdedNumberOption("Ability Cooldown", 30, 15, 60, 15, MiraNumberSuffixes.Seconds);
    
    public ModdedNumberOption AbilityDuration { get; } =
        new ModdedNumberOption("Ability Duration", 15, 10, 25, 5, MiraNumberSuffixes.Seconds);
    public ModdedNumberOption ZoomOutSize { get; } =
        new ModdedNumberOption("Zoom Out Size", 5, 4.5f, 8, 0.5f, MiraNumberSuffixes.None);
}