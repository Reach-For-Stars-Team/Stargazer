using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Cowboy;

public class WrangledModifier : TimedModifier
{
    public override string ModifierName => "Wrangled";

    public override float Duration => 20;

    public override bool ShowInFreeplay => true;

    private AnimationClip ogRun;
    
    private AnimationClip ogIdle;

    public override void OnActivate()
    {
        ogIdle = Player.MyPhysics.Animations.group.IdleAnim;
        ogRun = Player.MyPhysics.Animations.group.RunAnim;

        Player.MyPhysics.Animations.group.RunAnim = Assets.wrangledPlayerWalkAnim.LoadAsset();
        Player.MyPhysics.Animations.group.IdleAnim = Assets.wrangledPlayerWalkAnim.LoadAsset();
        Player.MyPhysics.Animations.Animator.Speed *= 4;
        Player.MyPhysics.Animations.PlayIdleAnimation();
        Player.MyPhysics.Speed *= OptionGroupSingleton<CowboyOptions>.Instance.WrangledPlayerSpeedMultiplier.Value;
        Player.cosmetics.gameObject.SetActive(false);
    }
    public override void OnDeactivate()
    {
        Player.MyPhysics.Animations.group.RunAnim = ogRun;
        Player.MyPhysics.Animations.group.IdleAnim = ogIdle;
        Player.MyPhysics.Animations.Animator.Speed /= 4;
        Player.MyPhysics.Animations.PlayIdleAnimation();
        Player.MyPhysics.Speed /= OptionGroupSingleton<CowboyOptions>.Instance.AbilityCooldown.Value;
        Player.cosmetics.gameObject.SetActive(true);
    }
}