using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Crewmates.Taskmaster;

public class TaskmasterOptions : AbstractOptionGroup<TaskmasterRole>
{
    public override string GroupName { get; } = "Taskmaster Settings";

    public ModdedEnumOption ValidTargets { get; set; } =
        new("Valid Targets", 0, typeof(AbilityTargets), new []
        {
            "Alive Players Only",
            "Dead Players Only",
            "Any"
        });

    public ModdedNumberOption AbilityCooldown { get; set; } =
        new("Ability Cooldown", 30f, 15f, 60f, 15f, MiraNumberSuffixes.Seconds);

    public enum AbilityTargets
    {
        AliveOnly = 0,
        DeadOnly = 1,
        Both = 2
    }
}