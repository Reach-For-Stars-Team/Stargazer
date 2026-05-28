using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Mole;

public class Dig : CustomActionButton
{
    public override string Name => "Dig";

    public override float Cooldown => OptionGroupSingleton<MoleOptions>.Instance.DigCD.Value;
    public override float EffectDuration => OptionGroupSingleton<MoleOptions>.Instance.TunnelingDuration.Value;

    public override ButtonLocation Location => ButtonLocation.BottomRight;

    public override int MaxUses => 0;

    public override LoadableAsset<Sprite> Sprite => Assets.DigButton;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is MoleRole;
    }

    public override bool CanUse()
    {
        return PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(1f, new ContactFilter2D().NoFilter()) == null;
    }

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcAddModifier<TunnelingModifier>();
    }
}