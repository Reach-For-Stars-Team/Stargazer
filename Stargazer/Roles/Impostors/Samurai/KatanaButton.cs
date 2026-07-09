using Hazel;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Roles.Impostors.Mole;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Samurai;

public class KatanaButton : CustomActionButton
{
    public override string Name => "Katana";

    public override float Cooldown => OptionGroupSingleton<SamuraiOptions>.Instance.KatanaCD.Value;
    public override float EffectDuration => OptionGroupSingleton<SamuraiOptions>.Instance.KatanaDuration.Value;

    public override ButtonLocation Location => ButtonLocation.BottomRight;

    public override int MaxUses => (int)OptionGroupSingleton<SamuraiOptions>.Instance.KatanaUses.Value;

    public override LoadableAsset<Sprite> Sprite => Assets.PlaceHolder;

    public override bool IsEffectCancellable()
    {
        return true;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is SamuraiRole;
    }
    
    public override bool CanUse()
    {
        return Timer < EffectDuration - 2 || Cooldown <= 0;
    }

    public override bool CanClick()
    {
        return CanUse();
    }

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcAddModifier<KatanaModifier>();
    }

    public override void OnEffectEnd()
    {
        PlayerControl.LocalPlayer.RpcRemoveModifier<KatanaModifier>();
    }
}