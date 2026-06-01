using System;
using System.Collections;
using System.Linq;
using AchievementsAPI.API;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Stargazer.Components;
using Stargazer.Utilities;
using Reactor.Utilities.Extensions;
using Stargazer.Features;
using UnityEngine;
using UnityEngine.Events;

namespace Stargazer.Roles.Impostors.Carrier;

public class CarryingModifier(byte PlayerId) : BaseModifier
{
    public override string ModifierName => "Carrying A Body";
    public override bool HideOnUi => true;
    public override bool ShowInFreeplay => false;
    public DeadBody Body = Helpers.GetBodyById(PlayerId);
    private SpriteRenderer Hand;

    public override void OnActivate()
    {
        Body.transform.SetParent(Player.transform);
        Body.transform.localPosition = new(0.25f, 1, 0);
        
        Hand = new GameObject("Hand").AddComponent<SpriteRenderer>();
        Hand.transform.parent = Player.transform;
        Hand.transform.localPosition = new(0, 0, -50);
        Player.StartCoroutine(Effects.Slide2D(Hand.transform, Vector2.zero, new(0f, 1), 0.1f));
        Hand.transform.localScale = new Vector3(.7f, .7f, 1f);
        
        Hand.sprite = Assets.HandHoldingBody.LoadAsset();
        Hand.material = new Material(Shader.Find("Unlit/PlayerShader"));
        PlayerMaterial.SetColors(Player.cosmetics.ColorId, Hand);
        
        Player.MyPhysics.Speed *= OptionGroupSingleton<CarrierOptions>.Instance.SpeedMultiplier.Value;

        // Disable body colliders to prevent interfering with vent triggers while carrying
        foreach (var col in Body.GetComponentsInChildren<Collider2D>())
            col.enabled = false;
    }

    public override void OnDeactivate()
    {
        Player.MyPhysics.Speed /= OptionGroupSingleton<CarrierOptions>.Instance.SpeedMultiplier.Value;
        
        if (Body) Body.transform.SetParent(null);
        Hand.gameObject.Destroy();

        // Re-enable body colliders now that we're done carrying
        if (Body)
        {
            foreach (var col in Body.GetComponentsInChildren<Collider2D>())
                col.enabled = true;
        }
        
        if (Player.MyPhysics.Velocity.x == 0)
        {
            Body.StartCoroutine(Effects.Sequence([
                Effects.Slide2DWorld(
                    Body.transform,
                    Body.transform.position,
                    Player.transform.position,
                    0.15f
                ),
                Effects.Bounce(Body.transform, 0.2f, 0.25f)
            ]));
        }
        else
        {
            Throw();
        }
    }

    public void Throw()
    {
        var options = OptionGroupSingleton<CarrierOptions>.Instance;

        // Use >= 0 so purely vertical movement (x == 0) defaults to throwing right
        float velX = Player.MyPhysics.Velocity.x;
        float distance = velX >= 0 ? options.ThrowDistance.Value : options.ThrowDistance.Value * -1;
        
        Vector2 origin = Player.GetTruePosition();
        Vector2 direction = distance > 0 ? Vector2.right : Vector2.left;
        
        LayerMask shipMask = LayerMask.GetMask("Ship");
        
        Physics2D.queriesHitTriggers = false;
        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction,
            Mathf.Abs(distance),
            shipMask
        );

        Physics2D.queriesHitTriggers = true;
        
        Vector2 targetPosition = hit.collider ? hit.point - (direction * 0.1f) : origin + direction * Mathf.Abs(distance);
        
        PlayerControl nearbyPlayer = PlayerControl.AllPlayerControls
            .ToArray()
            .Where(p => !p.Data.IsDead && Vector2.Distance((Vector2)p.transform.position, targetPosition) < 1f)
            .OrderBy(p => Vector2.Distance((Vector2)p.transform.position, targetPosition))
            .FirstOrDefault();

        Vent nearbyVent = ShipStatus.Instance.AllVents
            .Where(v => Vector2.Distance((Vector2)v.transform.position, targetPosition) < 1f)
            .OrderBy(v => Vector2.Distance((Vector2)v.transform.position, targetPosition))
            .FirstOrDefault();
        
        if (nearbyPlayer != null && options.ThrowOntoPlayer.Value)
        {
          targetPosition = nearbyPlayer.transform.position;
        }
        
        else if (nearbyVent != null && nearbyPlayer == null && options.ThrowInVent)
        {
            targetPosition = nearbyVent.transform.position;
        }
        
        Coroutines.Start(
            CoThrow(Body, Body.transform.position, targetPosition, 0.3f, new Action(() =>
                {
                    var options = OptionGroupSingleton<CarrierOptions>.Instance;
                    if (nearbyPlayer && options.ThrowOntoPlayer)
                    {
                        Coroutines.Start(RFSEffects.CoMoveArc(nearbyPlayer.transform, targetPosition, Vector2.Lerp(targetPosition, origin, 0.4f), 0.3f));
                        Player.StartCoroutine(Effects.Sequence(
                            Effects.Wait(0.1f),
                            Effects.Action(new System.Action((() => Player.CustomMurder(nearbyPlayer,
                                MurderResultFlags.DecisionByHost, teleportMurderer: false, showKillAnim: false))))
                        ));
                        if (Player.AmOwner) AchievementsTabSingleton<StargazerAchievements>.Instance.CarrierAchievement2.Unlock();
                    }
                    
                    else if (nearbyVent && options.ThrowInVent)
                    {
                        Body.bodyRenderers[0].enabled = false;
                        Body.transform.SetParent(nearbyVent.transform);
                        if (nearbyVent.EnterVentAnim) nearbyVent.myAnim.Play(nearbyVent.EnterVentAnim, 1f);
                        Coroutines.Start(RFSEffects.Boop(nearbyVent.transform, 0.3f, 2.5f, 0.3f));
                        if (Player.AmOwner) AchievementsTabSingleton<StargazerAchievements>.Instance.CarrierAchievement3.Unlock();
                    }
                })
        ));
    }

    public static IEnumerator CoThrow(DeadBody body, Vector2 start, Vector2 target, float duration, Action callback)
    {
        yield return Coroutines.Start(RFSEffects.CoMoveArc(body.transform, start, target, duration));
        Coroutines.Start(RFSEffects.Boop(body.transform, 0.3f, 1.5f, 0.2f));
        callback.Invoke();
        yield break;
    }
    
    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        ModifierComponent?.RemoveModifier(this);
    }
}