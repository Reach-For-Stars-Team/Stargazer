using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Impostors.Carrier;

public class CarrierOptions : AbstractOptionGroup<CarrierRole>
{
    public override string GroupName => "Carrier Options";
    public ModdedNumberOption CarryDuration { get; set; } = new("Max Carry Duration", 15f, 1f, 30f, 5f, MiraNumberSuffixes.Seconds, null, true);
    public ModdedNumberOption ThrowDistance { get; set; } = new("Throw Distance", 3f, 2f, 5f, 0.5f, MiraNumberSuffixes.None);
    public ModdedNumberOption SpeedMultiplier { get; set; } = new("Speed Multiplier while carrying body", 0.5f, 0.4f, 1f, 0.1f, MiraNumberSuffixes.Multiplier);
    public ModdedToggleOption ThrowInVent { get; set; } = new("Throw Into Vent", true);
    public ModdedToggleOption ThrowOntoPlayer { get; set; } = new("Throw On Player To Kill", false);
}