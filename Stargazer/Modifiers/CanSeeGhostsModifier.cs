using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using Stargazer.Roles.Neutrals.GhostBuster;
using Stargazer.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Modifiers;

public class CanSeeGhostsModifier : BaseModifier
{
    public override string ModifierName => "Can See Ghosts";

    public override bool HideOnUi => true;
    public override bool ShowInFreeplay => true;
    private SpriteRenderer overlay;
    public override void OnActivate()
    {
        if (!Player.AmOwner) return;
        
        HudManager.Instance.FadeScreen(Color.black, Color.clear, 0.4f);
        overlay = UnityObject.Instantiate(HudManager.Instance.FullScreen, HudManager.Instance.transform);
        overlay.transform.localPosition = new Vector3(0, 0, 10);
        overlay.gameObject.SetActive(true);
        var scaler = overlay.GetComponent<FullScreenScaler>();
        scaler.ScaleHeight = false;
        scaler.OnEnable();
        overlay.transform.localScale = new(overlay.transform.localScale.x, overlay.transform.localScale.x, 1);
        overlay.color = Color.white;
        overlay.sprite = Assets.GogglesOverlay.LoadAsset();
    }

    public override void OnDeactivate()
    {
        if (!Player.AmOwner) return;
        
        HudManager.Instance.FadeScreen(Color.black, Color.clear, 0.4f);
        overlay.gameObject.Destroy();
    }
}