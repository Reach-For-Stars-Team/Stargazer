using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Neutrals.GhostBuster;

public class GhostBusterOptions : AbstractOptionGroup<GhostBusterRole>
{
    public override string GroupName => "Ghost Buster Options";
    public ModdedNumberOption GooglesDuration { get; set; } = new ("Goggles Duration", 10f, 10f, 25f, 2.5f, MiraNumberSuffixes.Seconds);
    
    public ModdedNumberOption GhostQuota { get; set; } = new ("Ghosts Quota", 3, 3, 6, 1, MiraNumberSuffixes.None, "0 Ghosts");
}