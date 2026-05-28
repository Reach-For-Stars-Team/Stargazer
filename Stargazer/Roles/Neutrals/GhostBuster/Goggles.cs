using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Stargazer.Modifiers;
using UnityEngine;

namespace Stargazer.Roles.Neutrals.GhostBuster;

public class Goggles : CustomActionButton
{
    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcAddModifier<CanSeeGhostsModifier>();
    }

    public override void OnEffectEnd()
    {
        PlayerControl.LocalPlayer.RpcRemoveModifier<CanSeeGhostsModifier>();
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is GhostBusterRole && !role.Player.HasModifier<NeutralWinner>();
    }

    public override string Name => "Goggles";

    public override float Cooldown => 30;

    public override bool IsEffectCancellable()
    {
        return true;
    }

    public override float EffectDuration => OptionGroupSingleton<GhostBusterOptions>.Instance.GooglesDuration.Value;

    public override LoadableAsset<Sprite> Sprite => Assets.GogglesButton;
}