using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using PowerTools;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Carrier;

public class Carry : CustomActionButton<DeadBody>
{
    protected override void OnClick()
    {
        Button.OverrideText("Throw");
        PlayerControl.LocalPlayer.RpcAddModifier<CarryingModifier>(Target.ParentId);
    }

    public override void OnEffectEnd()
    {
        Button.OverrideText("Carry");
        PlayerControl.LocalPlayer.RpcRemoveModifier<CarryingModifier>();
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is CarrierRole;
    }
    public override string Name => "Carry";
    public override float Cooldown => 3;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override float EffectDuration => GetEffectDuration();

    private float GetEffectDuration()
    {
        float opt = OptionGroupSingleton<CarrierOptions>.Instance.CarryDuration.Value;
        return opt > 0 ? opt : int.MaxValue;
    }

    public override bool IsEffectCancellable()
    {
        return true;
    }

    public override float Distance => 1f;
    public override LoadableAsset<Sprite> Sprite => Assets.CarrierRoleIcon;
    public override DeadBody GetTarget()
    {
        return PlayerControl.LocalPlayer.GetNearestDeadBody(Distance);
    }

    public override void SetOutline(bool active)
    {
    }

    public override bool IsTargetValid(DeadBody target)
    {
        return base.IsTargetValid(target) && target.bodyRenderers[0].enabled;
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);
        Button?.cooldownTimerText.enabled = !(EffectActive && Mathf.Approximately(GetEffectDuration(), int.MaxValue));
    }
}