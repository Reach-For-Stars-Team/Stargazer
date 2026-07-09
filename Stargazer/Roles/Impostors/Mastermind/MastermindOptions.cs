using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Impostors.Mastermind;

public class MastermindOptions : AbstractOptionGroup<MastermindRole>
{
    public override string GroupName => "Mastermind Options";
    public ModdedToggleOption CanKillBeforeRecruiting { get; set; } = new("Can Kill Before Recruiting", false);
    public ModdedNumberOption RecruitCooldown { get; set; } = new("Recruit Cooldown", 25f, 0f, 120f, 5f, MiraNumberSuffixes.Seconds, null, false);
    public ModdedToggleOption RecruitedCanVent { get; set; } = new("Recruited Players Can Vent", false);
    public ModdedNumberOption RecruitedKillCooldown { get; set; } = new("Recruited Kill Cooldown", 30f, 0f, 120f, 5f, MiraNumberSuffixes.Seconds, null, false);
    public ModdedNumberOption RecruitedKillRange { get; set; } = new("Recruited Kill Range", 1.5f, 0.5f, 2f, 0.25f, MiraNumberSuffixes.Multiplier, null, false);
    public ModdedNumberOption RecruitRange { get; set; } = new("Recruit Range", 2f, 0.5f, 3f, 0.25f, MiraNumberSuffixes.Multiplier, null, false);
    
}