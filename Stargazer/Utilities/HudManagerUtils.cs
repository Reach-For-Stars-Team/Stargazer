using System;
using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using MiraAPI.Utilities;

namespace Stargazer.Utilities;

public static class HudManUtils
{
    public static HideAndSeekDeathPopup SpawnHnSPopUp(this HudManager Hudman, PlayerControl source, string Sentence)
    {
        var popup = GameManagerCreator.Instance.HideAndSeekManagerPrefab.DeathPopupPrefab;
        UnityObject.Instantiate(popup, HudManager.Instance.transform.parent).Show(source, 5);

        var textobj = popup.transform.GetChild(0);
        textobj.GetComponent<TextMeshPro>().text = Sentence;

        UnityObject.DestroyImmediate(textobj.GetComponent<TextTranslatorTMP>());

        return popup;
    }

    public static void SpawnTextOverlay(this HudManager hudman, string Text)
    {
        var textOverlay = UnityObject.Instantiate(hudman.TaskCompleteOverlay, hudman.TaskCompleteOverlay.parent)
            .GetComponent<TextMeshPro>();

        textOverlay.GetComponent<TextTranslatorTMP>().DestroyImmediate();
        textOverlay.text = Text;

        Coroutines.Start(AnimateObjectOnScreen(textOverlay.gameObject));
    }

    private static IEnumerator AnimateObjectOnScreen(GameObject go)
    {
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(HudManager.Instance.TaskCompleteSound, false);
        go.SetActive(true);
        yield return Effects.Slide2D(go.transform, new Vector2(0f, -8f), Vector2.zero, 0.25f);
        for (var time = 0f; time < 0.75f; time += Time.deltaTime) yield return null;
        yield return Effects.Slide2D(go.transform, Vector2.zero, new Vector2(0f, 8f), 0.25f);
        go.DestroyImmediate();
        yield break;
    }

    public static void ShowKillOverlay(this HudManager hudman, NetworkedPlayerInfo Killer,
        NetworkedPlayerInfo ToBeKilled, Color color)
    {
        hudman.KillOverlay.ShowKillAnimation(Killer, ToBeKilled);
        hudman.KillOverlay.flameParent.transform.GetChild(0).GetComponent<SpriteRenderer>().color = color;
    }

    public static IEnumerator CoShowKillOverlay(this HudManager hudman, NetworkedPlayerInfo Killer,
        NetworkedPlayerInfo ToBeKilled, Color color)
    {
        hudman.KillOverlay.ShowKillAnimation(Killer, ToBeKilled);

        SpriteRenderer Flame = hudman.KillOverlay.flameParent.transform.GetChild(0).GetComponent<SpriteRenderer>();
        Flame.color = color;
        yield return new WaitForSeconds(3f);
        Flame.color = Color.white;
    }

    public static void FlashScreen(this HudManager hudman, Color color, AudioClip sound = null)
    {
        Coroutines.Start(CoFlashScreen(hudman, color, sound));
    }

    public static IEnumerator CoFlashScreen(this HudManager hudman, Color color, AudioClip sound)
    {
        WaitForSeconds wait = new WaitForSeconds(1f);
        bool light = false;
        hudman.FullScreen.color = new(color.r, color.g, color.b, 0.372549027f);
        hudman.FullScreen.gameObject.SetActive(!hudman.FullScreen.gameObject.activeSelf);
        SoundManager.Instance.PlaySound(sound, false, 1f, null);
        if (hudman.lightFlashHandle == null)
        {
            hudman.lightFlashHandle = DestroyableSingleton<DualshockLightManager>.Instance.AllocateLight();
            hudman.lightFlashHandle.color = color;
            hudman.lightFlashHandle.intensity = 1f;
        }

        light = !light;
        yield return wait;
        hudman.FullScreen.gameObject.SetActive(false);
        yield break;
    }

    public static void FadeScreen(this HudManager hudman, Color source, Color target, float duration)
    {
        Coroutines.Start(CoFadeScreen(source, target, duration));
    }
    private static IEnumerator CoFadeScreen(Color source, Color target, float duration)
    {
        var overlay = UnityObject.Instantiate<SpriteRenderer>(HudManager.Instance.FullScreen, HudManager.Instance.transform);
        overlay.gameObject.SetActive(true);
        yield return Effects.ColorFade(overlay, source, target, duration);
        overlay.gameObject.Destroy();
        yield break;
    }

    public static AbilityButton CreateAbilityButton(Transform parent, string Name, LoadableResourceAsset Sprite, Action OnClick)
    {
        var Button = UnityObject.Instantiate(HudManager.Instance.AbilityButton, parent);
        Button.name = Name + "Button";
        Button.OverrideText(Name.ToUpperInvariant());

        Button.graphic.sprite = Sprite.LoadAsset();

        var pb = Button.GetComponent<PassiveButton>();
        pb.OnClick = new Button.ButtonClickedEvent();
        pb.OnClick.AddListener((UnityAction)(() =>
        {
            OnClick.Invoke();
        }));

        return Button;
    }

    public static void ToggleCameraShadows(this HudManager hudman, bool fadeOut)
    {
        if (fadeOut) Coroutines.Start(RFSEffects.MaterialColorFade(hudman.ShadowQuad, RFSPalette.ShadowQuadColor, Color.clear,
            0.7f));
        
        else Coroutines.Start(RFSEffects.MaterialColorFade(hudman.ShadowQuad, Color.clear, RFSPalette.ShadowQuadColor, 
            1.4f));
    }
}