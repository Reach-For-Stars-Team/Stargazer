using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Neutrals.Roleless;

public class RolelessOptions : AbstractOptionGroup<RolelessRole>
{
    public override string GroupName => "Pirate Options";

    public ModdedNumberOption AbilityUses { get; set; } =
        new("Ability Uses", 0, 0, 5, 1, MiraNumberSuffixes.None, "0 Uses", true);
    
    public ModdedToggleOption SuicideWhenMisguess { get; set; } =
        new("Suicide when misguessing", true);
}