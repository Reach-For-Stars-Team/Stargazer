using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Networking;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Cowboy;

public class Lasso : CustomActionButton
{
    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.transform.GetComponent<HnSImpostorScreamSfx>().LocalImpostorYeehaw();
        var target = PlayerControl.LocalPlayer.GetClosestPlayer(false, 1000f, true);
        PlayerControl.LocalPlayer.RpcLasso(target);
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is CowboyRole;
    }

    public override string Name => "Lasso";

    public override float Cooldown => OptionGroupSingleton<CowboyOptions>.Instance.AbilityCooldown.Value;

    public override float EffectDuration => OptionGroupSingleton<CowboyOptions>.Instance.PullDuration.Value;

    public override LoadableAsset<Sprite> Sprite => Assets.LassoButton;
}