using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Paranoiac;

public class ParanoiaAbility : CustomActionButton
{
    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.AddModifier<ParanoidModifier>();
    }

    public override void OnEffectEnd()
    {
        PlayerControl.LocalPlayer.RemoveModifier<ParanoidModifier>();
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is ParanoiacRole;
    }

    public override string Name => "Paranoia";

    public override Color TextOutlineColor => RFSPalette.ParanoiacRoleColor;

    public override float Cooldown => OptionGroupSingleton<ParanoiacOptions>.Instance.AbilityCooldown.Value;

    public override float EffectDuration => OptionGroupSingleton<ParanoiacOptions>.Instance.AbilityDuration.Value;

    public override LoadableAsset<Sprite> Sprite => Assets.ParanoiaButton;
}