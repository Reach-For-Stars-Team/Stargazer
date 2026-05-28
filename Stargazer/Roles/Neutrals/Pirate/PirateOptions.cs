using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Neutrals.Pirate;

public class PirateOptions : AbstractOptionGroup<PirateRole>
{
    public override string GroupName => "Pirate Options";

    public ModdedNumberOption GoldPerTask { get; set; } =
        new("Gold Per Task Completed", 25, 50, 75, 25, MiraNumberSuffixes.None, "0 Gold");
    
    public ModdedNumberOption GoldPerSteal { get; set; } =
        new("Gold Per Stolen Player", 100, 50, 150, 25, MiraNumberSuffixes.None, "0 Gold");
    
    public ModdedNumberOption GoldPerTreasure { get; set; } =
        new("Gold Per Treasure", 250, 150, 350, 25, MiraNumberSuffixes.None, "0 Gold");

    public ModdedNumberOption MurderChanceWhenStealing { get; set; }  = new("Murder chance when stealing", 40, 0, 100, 10, "0",
        "0", MiraNumberSuffixes.Percent, halfIncrements: true);
}