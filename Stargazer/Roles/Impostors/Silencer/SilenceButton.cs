using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Stargazer.Components.Tasks;
using Stargazer.Networking;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Silencer;

public class SilenceButton : CustomActionButton
{
    public override string Name => "Silence";

    public override float Cooldown => OptionGroupSingleton<SilencerOptions>.Instance.AbilityCooldown.Value;

    public override float EffectDuration => OptionGroupSingleton<SilencerOptions>.Instance.AbilityDuration.Value;

    public override LoadableAsset<Sprite> Sprite => Assets.SilenceButton;

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcSilence();
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is SilencerRole;
    }

    public override bool CanUse()
    {
        return !PlayerTask.PlayerHasTaskOfType<SilenceTask>(PlayerControl.LocalPlayer);
    }
}