using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Crewmates.Sheriff;

public class SheriffOptions : AbstractOptionGroup<SheriffRole>
{
    public override string GroupName => "Sheriff Options";

    public ModdedNumberOption TimeframePerBullet { get; } = new("Timeframe Per Bullet", 0.8f, 0.65f, 1, 0.05f, MiraNumberSuffixes.Seconds);
}