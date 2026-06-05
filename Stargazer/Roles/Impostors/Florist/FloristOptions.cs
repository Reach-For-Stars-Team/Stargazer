using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Impostors.Florist;

public class FloristOptions : AbstractOptionGroup<FloristRole>
{
    public override string GroupName => "Florist Options";

    public ModdedNumberOption PlantCooldown { get; } =
        new ModdedNumberOption("Plant Cooldown", 30, 5, 90, 5, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption ControlCooldown { get; } =
        new ModdedNumberOption("Control Cooldown", 10, 5, 30, 5, MiraNumberSuffixes.Seconds);
        
    public ModdedNumberOption ControlDuration { get; } =
        new ModdedNumberOption("Control Duration", 10, 5, 30, 5, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption BlossomTime { get; } =
        new ModdedNumberOption("Exposure to flowers needed to be controlled", 3, 3, 5, 1, MiraNumberSuffixes.Seconds);
    
    public ModdedToggleOption ResetBlossomAfterControl { get; } =
        new ModdedToggleOption("Reset exposure after having been controlled", true);
}