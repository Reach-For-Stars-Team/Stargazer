using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Modifiers.Game.Crewmates.Workaholic;

public class WorkaholicModifierOptions : AbstractOptionGroup<WorkaholicModifier>
{
    public override string GroupName => "Workaholic Modifier Options";

    public override bool ShowInModifiersMenu => true;

    public ModdedNumberOption AmountPerGame { get; set; } =
        new ModdedNumberOption("Amount per Game", 0, 0, 15, 1, MiraNumberSuffixes.None);

    public ModdedNumberOption AssignmentChance { get; set; } = new ModdedNumberOption("Assignment Chance", 0, 0, 100,
        10, "0", "0", MiraNumberSuffixes.Percent, "0", true);

    public ModdedNumberOption TasksQuota { get; set; } = new ModdedNumberOption("Task Quota", 1, 1, 3,
        1, MiraNumberSuffixes.None);
}