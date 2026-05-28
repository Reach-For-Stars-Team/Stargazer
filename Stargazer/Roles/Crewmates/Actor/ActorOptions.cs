using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Crewmates.Actor;

public class ActorOptions : AbstractOptionGroup<ActorRole>
{
    public override string GroupName => "Actor Options";

    public ModdedNumberOption ActCD { get; } = new("Act Cooldown", 25, 15, 45, 5, MiraNumberSuffixes.Seconds);
}