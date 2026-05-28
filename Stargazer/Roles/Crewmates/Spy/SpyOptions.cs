using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Crewmates.Spy;

public class SpyOptions : AbstractOptionGroup<SpyRole>
{
    public override string GroupName => "Spy Options";

    public ModdedNumberOption BatteryDuration { get; } = new("Battery Duration", 5, 5, 30, 5, MiraNumberSuffixes.Seconds);
}