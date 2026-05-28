using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Crewmates.Lifesaver;

public class LifesaverOptions : AbstractOptionGroup<LifesaverRole>
{
    public override string GroupName => "Lifesaver Options";

    public ModdedNumberOption AbilityCooldown { get; } =
        new ModdedNumberOption("Ability Cooldown", 30, 15, 60, 15, MiraNumberSuffixes.Seconds);
}