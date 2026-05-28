using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Stargazer.Utilities;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Observer;

public class ObserveButton : CustomActionButton
{
    protected override void OnClick()
    {
        CameraUtils.ZoomInOut(OptionGroupSingleton<ObserverOptions>.Instance.ZoomOutSize.Value, 0.7f);
        HudManager.Instance.ToggleCameraShadows(true);
    }

    public override void OnEffectEnd()
    {
        CameraUtils.ZoomInOut(3, 0.4f);
        HudManager.Instance.ToggleCameraShadows(false);
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is ObserverRole;
    }

    public override string Name => "Observe";

    public override float Cooldown => OptionGroupSingleton<ObserverOptions>.Instance.AbilityCooldown.Value;

    public override float EffectDuration => OptionGroupSingleton<ObserverOptions>.Instance.AbilityDuration.Value;

    public override LoadableAsset<Sprite> Sprite => Assets.ObserveButton;
}