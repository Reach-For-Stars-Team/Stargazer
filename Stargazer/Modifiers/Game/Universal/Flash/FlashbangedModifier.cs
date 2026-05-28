using System;
using System.Collections;
using System.Collections.Generic;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Map;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using Stargazer.Utilities;
using Reactor.Utilities;
using UnityEngine;
using UnityEngine.U2D;

namespace Stargazer.Modifiers.Game.Universal.Flash;

public class FlashbangedModifier : TimedModifier
{
    public override string ModifierName => "Flashbanged";

    public override float Duration => 10;

    public override bool ShowInFreeplay => true;

    public override void OnActivate()
    {
        if (!Player.AmOwner) return;

        Coroutines.Start(CoAnimateFlash());

        for (int i = 0; i < 4; i++)
        {
            var shadowQuad = UnityObject.Instantiate(HudManager.Instance.ShadowQuad, Camera.main.transform);
            shadowQuad.transform.localScale = HudManager.Instance.ShadowQuad.transform.localScale;
            Coroutines.Start(RFSEffects.MaterialColorFadeAndDestroy(shadowQuad, Color.black, Color.clear, 8));
            Player.StartCoroutine(Effects.Slide2D(shadowQuad.transform, new Vector2(UnityRandom.RandomRange(-5, 5), UnityRandom.RandomRange(-5, 5)), new Vector2(UnityRandom.RandomRange(-5, 5), UnityRandom.RandomRange(-5, 5)), 4f));
        }
    }

    public IEnumerator CoAnimateFlash()
    {
        var rend = new GameObject("Flash").AddComponent<SpriteRenderer>();
        rend.sprite = Assets.Circle.LoadAsset();
        rend.gameObject.layer = LayerMask.NameToLayer("UI");
        rend.transform.parent = HudManager.Instance.transform;
        rend.transform.position = Player.transform.position;
        yield return Player.StartCoroutine(Effects.ScaleIn(rend.transform, 0, 20, 0.7f));
        yield return new WaitForSeconds(1f);
        yield return Coroutines.Start(RFSEffects.ColorFadeAndDestroy(rend, Color.white, Color.clear, 3.3f));
        yield break;
    }
}