using Il2CppSystem;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Components.Minigames;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Spy;

public class Watch : CustomActionButton
{
    public override string Name => "Watch";
    public override float Cooldown => 0;
    public override float EffectDuration => 0;

    public override int MaxUses => 0;

    public override LoadableAsset<Sprite> Sprite => Assets.WatchButton;
    public override ButtonLocation Location => ButtonLocation.BottomRight;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is SpyRole && Target != null;
    }
    public PlayerControl Target;
    public override bool CanUse()
    {
        return SpyMinigame.Instance == null && PlayerControl.LocalPlayer.Data.Role is SpyRole s && s.CurrentCharge >= 0 && Target != null;
    }

    protected override void OnClick()
    {
        SpyMinigame.CreateAndOpen();
        Target.AddModifier<BeingSpiedOnModifier>(PlayerControl.LocalPlayer);
    }
}