using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Crewmates.Spirit;

public class SpiritOptions : AbstractOptionGroup<SpiritRole>
{
    public override string GroupName => "Spirit Options";

    public ModdedNumberOption Duration { get; } = new("Out of body duration", 5, 5, 15, 2.5f, MiraNumberSuffixes.Seconds);
    
    public ModdedEnumOption OnEffectEndResult { get; } = new("On Effect End", 1, typeof(EffectEndResults));

    public enum EffectEndResults
    {
        ReturnToBody = 0,
        Die = 1
    }
}