using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace Stargazer.Roles.Crewmates.Lightener;

public class LightenerOptions : AbstractOptionGroup<LightenerRole>
{
    public override string GroupName { get; } = "Lightener Settings";

    public ModdedNumberOption LightenUpUses { get; set; } =
        new("Lantern Uses", 3f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption LightenUpCD { get; set; } =
        new("Ability Cooldown", 30f, 15f, 60f, 15f, MiraNumberSuffixes.None);
    
    public ModdedNumberOption LightRadius { get; set; } =
        new("Light Radius", 2f, 1.5f, 4f, 0.5f, MiraNumberSuffixes.None);
}