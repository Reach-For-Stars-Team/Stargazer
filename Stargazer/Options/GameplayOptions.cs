using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using UnityEngine;

namespace Stargazer.Options;

public class GameplayOptions : AbstractOptionGroup
{
    public override string GroupName => "Gameplay Options";

    public ModdedToggleOption PitchBlackShadow { get; set; } = new("Pitch black shadows", false)
    {
        ChangedEvent = new(b =>
        {
            if (b) HudManager.Instance.ShadowQuad.material.color = Color.black;
            else HudManager.Instance.ShadowQuad.material.color = new(0.245f, 0.245f, 0.245f);
        })
    };
}