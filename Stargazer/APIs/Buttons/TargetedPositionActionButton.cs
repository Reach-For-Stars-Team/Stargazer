using System;
using MiraAPI.Hud;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.APIs.Buttons;

[MiraIgnore]
public abstract class TargetedPositionActionButton : CustomActionButton
{
    //TODO Android compat? pretty sure the pointer doesn't exist on android
    public SpriteRenderer targetRenderer;
    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }
        
        EffectActive = !EffectActive;
        
        OnClick();
        
        if (EffectActive)
        {
            PlayerControl.LocalPlayer.NetTransform.Halt();
            PlayerControl.LocalPlayer.moveable = false;
            targetRenderer = new GameObject().AddComponent<SpriteRenderer>();
            targetRenderer.color = new(1, 1, 1, 0.5f);
            targetRenderer.gameObject.layer = LayerMask.NameToLayer("UI");
            targetRenderer.transform.position = PlayerControl.LocalPlayer.GetTruePosition();
            targetRenderer.sprite = TargetSprite.LoadAsset();
            var btn = targetRenderer.gameObject.AddComponent<PassiveButton>();
            btn.ClickMask = targetRenderer.gameObject.AddComponent<CircleCollider2D>();
            btn.Colliders = new([btn.ClickMask]);
            btn.OnClick = new();
            btn.OnClick.AddListener(new Action(() =>
            {
                if (!IsTargetValid(targetRenderer.transform.position)) return;
                PlayerControl.LocalPlayer.moveable = true;
                OnSelectTargetPosition(targetRenderer.transform.position);
                EffectActive = false;
                ResetCooldownAndOrEffect();
                targetRenderer.gameObject.Destroy();
                Timer = Cooldown;
            }));
            btn.OnMouseOut = new();
            btn.OnMouseOver = new();
        }
        else
        {
            targetRenderer.gameObject.Destroy();
        }
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        base.FixedUpdateHandler(playerControl);
        if (EffectActive && targetRenderer != null)
        {
            if (!OperatingSystem.IsAndroid()) targetRenderer.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            else
            {
                targetRenderer.transform.position += (Vector3) HudManager.Instance.joystick.DeltaL * Time.deltaTime * 4;
            }
        }
    }

    public abstract void OnSelectTargetPosition(Vector2 target);

    public abstract bool IsTargetValid(Vector2 pos);
    
    public abstract LoadableAsset<Sprite> TargetSprite { get; }
}