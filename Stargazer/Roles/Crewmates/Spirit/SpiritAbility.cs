using Il2CppSystem;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Components.Minigames;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Spirit;

public class SpiritAbility : CustomActionButton
{
    public override string Name => "Spirit";
    
    public override float Cooldown => 0;
    
    public override float EffectDuration => OptionGroupSingleton<SpiritOptions>.Instance.Duration.Value;

    public override int MaxUses => 0;

    public override LoadableAsset<Sprite> Sprite => Assets.GhostFormButton;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is SpiritRole;
    }

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcAddModifier<SpiritGhostModifier>();
    }

    public override void OnEffectEnd()
    {
        PlayerControl.LocalPlayer.RpcRemoveModifier<SpiritGhostModifier>();
    }
}