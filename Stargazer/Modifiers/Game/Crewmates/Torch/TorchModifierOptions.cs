using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Modifiers.Game.Crewmates.Torch;

public class TorchModifierOptions : AbstractOptionGroup<TorchModifier>
{
    public override string GroupName => "Torch Modifier Options";

    public override bool ShowInModifiersMenu => true;

    public ModdedNumberOption AmountPerGame { get; set; } =
        new ModdedNumberOption("Amount per Game", 0, 0, 15, 1, MiraNumberSuffixes.None);

    public ModdedNumberOption AssignmentChance { get; set; } = new ModdedNumberOption("Assignment Chance", 0, 0, 100,
        10, "0", "0", MiraNumberSuffixes.Percent, "0", true);

    public ModdedNumberOption FlashlightSize { get; set; } = new ModdedNumberOption("Flashlight Size", 1, 1, 3,
        0.5f, MiraNumberSuffixes.Multiplier);
}