using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Impostors.Samurai;

public class SamuraiOptions : AbstractOptionGroup<SamuraiRole>
{
    public override string GroupName => "Mole Options";

    public ModdedNumberOption KatanaDuration { get; set; } =
        new("Katana Duration", 10f, 10f, 25f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption KatanaCD { get; set; } =
        new("Katana Cooldown", 30f, 15f, 60f, 15f, MiraNumberSuffixes.Seconds);
    public ModdedNumberOption KatanaSpeedMultiplier { get; set; } =
        new("Katana Speed Multiplier", 1.5f, 1f, 2f, 0.25f, MiraNumberSuffixes.Multiplier);
    public ModdedNumberOption KatanaUses { get; set; } =
        new("Katana Uses", 0, 0, 10, 1, MiraNumberSuffixes.None, zeroInfinity:true);

    public ModdedToggleOption KatanaKillImps { get; set; } =
        new("Katana Kills Imps", false);
    public ModdedToggleOption KatanaCancelable { get; set; } =
        new("Katana Cancelable", false);
    public ModdedNumberOption KillLimitPerKatanaUse { get; set; } =
        new("Kill Limit Per Katana Use", 3, 1, 5, 1, MiraNumberSuffixes.None);
}