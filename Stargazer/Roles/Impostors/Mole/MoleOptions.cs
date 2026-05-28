using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Impostors.Mole;

public class MoleOptions : AbstractOptionGroup<MoleRole>
{
    public override string GroupName => "Mole Options";

    public ModdedNumberOption TunnelingDuration { get; set; } =
        new("Tunneling Duration", 10f, 10f, 25f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption DigCD { get; set; } =
        new("Dig Cooldown", 30f, 15f, 60f, 15f, MiraNumberSuffixes.Seconds);
}