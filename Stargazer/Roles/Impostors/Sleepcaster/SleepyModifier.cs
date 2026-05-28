using System.Collections;
using MiraAPI.Modifiers.Types;
using Stargazer.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Sleepcaster;

public class SleepyModifier : TimedModifier
{
    public override string ModifierName => "Sleepy";

    public override float Duration => 10;

    public override bool ShowInFreeplay => true;

    private GameObject SleepOverlay;

    private TextMeshPro TimeRemainingText;

    public override void OnActivate()
    {
        Player.NetTransform.Halt();
        Player.moveable = false;
        Player.AnimateCustom(Assets.SleepingPlayerAnimation.LoadAsset());
        Player.cosmetics.gameObject.SetActive(false);
        if (Player.AmOwner)
        {
            Coroutines.Start(CoAnimate());
        }
    }

    public IEnumerator CoAnimate()
    {
        HudManager.Instance.FadeScreen(Color.clear, Color.black, 0.3f);
        yield return new WaitForSeconds(0.3f);
        HudManager.Instance.SetHudActive(false);
        SleepOverlay = UnityObject.Instantiate(Assets.SleepOverlay.LoadAsset(), HudManager.Instance.transform);
        SleepOverlay.transform.localPosition = new(0, 0, -100);
        TimeRemainingText = SleepOverlay.transform.Find("DurationText").GetComponent<TextMeshPro>();
            
        foreach (var tmp in SleepOverlay.GetComponentsInChildren<TextMeshPro>())
        {
            tmp.fontStyle = FontStyles.Bold;
        }
        yield break;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (TimeRemainingText != null)
        {
            TimeRemainingText.text = $"Time left: {(int)TimeRemaining}s";
        }
    }

    public override void OnDeactivate()
    {
        Player.NetTransform.Halt();
        Player.moveable = true;
        Player.cosmetics.gameObject.SetActive(true);
        Player.MyPhysics.Animations.PlayIdleAnimation();
        if (Player.AmOwner)
        {
            SleepOverlay.Destroy();
            HudManager.Instance.SetHudActive(false);
        }
    }

    public override void OnMeetingStart()
    {
        ModifierComponent?.RemoveModifier(this);
    }
    
    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }
}