using System.Collections;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Stargazer.Modifiers;
using Stargazer.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;


namespace Stargazer.Roles.Impostors.Samurai;

public class KatanaModifier : TimedModifier
{
    public override string ModifierName => "Katana";
    public override float Duration => OptionGroupSingleton<SamuraiOptions>.Instance.KatanaDuration.Value;
    public override bool HideOnUi => true;
    private bool KillImps = OptionGroupSingleton<SamuraiOptions>.Instance.KatanaKillImps.Value;
    private int KillsThisUse = 0;
    
    
    private AnimationClip ogRun;
    
    private AnimationClip ogIdle;

    private float ogSpeed;

    public void CheckForBumps()
    {
        PlayerControl closestPlayer = Player.GetClosestPlayer(KillImps, 0.3f);
        if (closestPlayer != null)
        {
            Player.RpcCustomMurder(closestPlayer, true, false, true, false);
            KillsThisUse++;
            if (KillsThisUse >= OptionGroupSingleton<SamuraiOptions>.Instance.KillLimitPerKatanaUse.Value)
            {
                Player.RpcRemoveModifier<KatanaModifier>();
            }
        }
        
    }
    public override void OnActivate()
    {
        ogIdle = Player.MyPhysics.Animations.group.IdleAnim;
        ogRun = Player.MyPhysics.Animations.group.RunAnim;

        Player.MyPhysics.Animations.group.RunAnim = Assets.wrangledPlayerWalkAnim.LoadAsset();
        Player.MyPhysics.Animations.group.IdleAnim = Assets.wrangledPlayerWalkAnim.LoadAsset();
        Player.MyPhysics.Animations.Animator.Speed *= 4;
        Player.MyPhysics.Animations.PlayIdleAnimation();
        ogSpeed = Player.MyPhysics.Speed;
        Player.MyPhysics.Speed *= OptionGroupSingleton<SamuraiOptions>.Instance.KatanaSpeedMultiplier.Value;
        Player.cosmetics.gameObject.SetActive(false);
        KillsThisUse = 0;
        if (!Player.AmOwner) return;
        HudManager.Instance.KillButton.Hide();
        HudManager.Instance.ImpostorVentButton.Hide();
        
    }
    public override void OnDeactivate()
    {
        Player.MyPhysics.Animations.group.RunAnim = ogRun;
        Player.MyPhysics.Animations.group.IdleAnim = ogIdle;
        Player.MyPhysics.Animations.Animator.Speed /= 4;
        Player.MyPhysics.Animations.PlayIdleAnimation();
        Player.MyPhysics.Speed = ogSpeed;
        Player.cosmetics.gameObject.SetActive(true);
        KillsThisUse = 0;
        if (!Player.AmOwner) return;
        HudManager.Instance.KillButton.Show();
        HudManager.Instance.ImpostorVentButton.Show();
    }

    public override void FixedUpdate()
    {
        CheckForBumps();
        base.FixedUpdate();
    }
}