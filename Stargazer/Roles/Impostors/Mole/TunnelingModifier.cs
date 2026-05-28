using System.Collections;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using Stargazer.Modifiers;
using Stargazer.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Mole;

public class TunnelingModifier : TimedModifier
{
    public override string ModifierName => "Tunneling";
    public override float Duration => 10;
    private Vent firstVent;
    private Vent secondVent;
    public override void OnActivate()
    {
        firstVent = CreateVent();
        Coroutines.Start(CoAnimate());
        Player.cosmetics.nameTextContainer.SetActive(false);
        Player.cosmetics.TogglePetVisible(false);
        
        if (!Player.AmOwner) return;
        HudManager.Instance.FadeScreen(Color.black, Color.clear, 0.2f);
        HudManager.Instance.PlayerCam.ShakeScreen(Duration, 0.7f);
        HudManager.Instance.KillButton.Hide();
        HudManager.Instance.ImpostorVentButton.Hide();
    }

    public IEnumerator CoAnimate(int numFramesUntilDisappearance = 16)
    {
        Player.cosmetics.gameObject.SetActive(false);
        Player.MyPhysics.Animations.Animator.Play(Assets.DigAnimation.LoadAsset(), 1f);
        if (numFramesUntilDisappearance > 0)
        {
            while (Player.MyPhysics.Animations.Animator.FrameTime < numFramesUntilDisappearance)
            {
                yield return null;
            }
            Player.MyPhysics.Animations.Animator.Stop();
        }
        else
        {
            while (Player.MyPhysics.Animations.Animator.IsPlaying())
            {
                yield return null;
            }
        }
        yield break;
    }

    public override void OnDeactivate()
    {
        secondVent = CreateVent();
        firstVent.Center = secondVent;
        secondVent.Center = firstVent;
        Player.cosmetics.nameTextContainer.SetActive(true);
        Player.cosmetics.TogglePetVisible(true);
        Player.cosmetics.gameObject.SetActive(true);
        Player.StartCoroutine(Player.MyPhysics.Animations.CoPlayExitVentAnimation());
        
        if (!Player.AmOwner) return;
        SoundManager.Instance.StopAllSound();
        CustomButtonSingleton<Dig>.Instance.ResetCooldownAndOrEffect();
        HudManager.Instance.KillButton.Show();
        HudManager.Instance.ImpostorVentButton.Show();
    }

    private Vent CreateVent()
    {
        if (Player.Data.Role is MoleRole mole)
        {
            var vent = UnityObject.Instantiate(ShipStatus.Instance.AllVents[0]);
            vent.Center = null;
            vent.Left = null;
            vent.Right = null;
            Vector3 ppos = Player.transform.position;
            vent.transform.localPosition = new Vector3(1.2f, 1.2f, 1);
            vent.transform.position = new Vector3(ppos.x, ppos.y - 0.1f, 1f);
            vent.myRend.material = new(ShipStatus.Instance.EmergencyButton.GetComponent<SpriteRenderer>().material.shader);
            ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out ISystemType system);
            vent.UpdateArrows(system.TryCast<VentilationSystem>());
            vent.Id = VentUtils.GetAvailableId();
            var newAllVents = ShipStatus.Instance.AllVents.ToList();
            newAllVents.Add(vent);
            ShipStatus.Instance.AllVents = newAllVents.ToArray();
            mole.MinedVents.Add(vent);
            ShipStatus.Instance.StartCoroutine(Effects.Bloop(0.2f, vent.transform));
            return vent;
        }

        return null;
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        Player.lightSource.SetViewDistance(1);
    }

    public void PlayerPhysicsFixedUpdate()
    {
        bool flag = Player.Data != null && Player.Data.IsDead;
        if (Player.AmOwner)
        {
            if (Player.CanMove && GameData.Instance && HudManager.Instance && HudManager.Instance.joystick != null && HudManager.Instance.joystick.DeltaL != Vector2.zero)
            {
                Player.MyPhysics.SetNormalizedVelocity((HudManager.Instance.joystick.DeltaL + Player.MyPhysics.Velocity.normalized));
            }
            Player.MyPhysics.CheckCancelPetting();
        }
    }
}