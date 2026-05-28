using System.Linq;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using Stargazer.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Pyromaniac;

public class DousedModifier : BaseModifier
{
    public override string ModifierName => "Doused";
    public override bool ShowInFreeplay => true;
    public PlayerControl PyromaniacPlayer;
    private SpriteRenderer overlay;
    public DousedModifier(PlayerControl source)
    {
        PyromaniacPlayer = source;
    }

    public override void OnActivate()
    {
        if (Player.AmOwner)
        {
            overlay = UnityObject.Instantiate(HudManager.Instance.FullScreen, HudManager.Instance.transform);
            overlay.transform.localPosition = new Vector3(0, 0, 10);
            overlay.gameObject.SetActive(true);
            var scaler = overlay.GetComponent<FullScreenScaler>();
            scaler.ScaleHeight = false;
            scaler.OnEnable();
            overlay.transform.localScale = new(overlay.transform.localScale.x, overlay.transform.localScale.x, 1);
            overlay.sprite = Assets.DousedOverlay.LoadAsset();
            Player.StartCoroutine(Effects.ColorFade(overlay, new(1, 1, 1, 0), Color.white, 20));
        }

        if (PyromaniacPlayer && PyromaniacPlayer.AmOwner)
        {
            Player.cosmetics.nameText.color = RFSPalette.DousedPlayerNameColor;
            Player.cosmetics.bodySprites[0].BodySprite.material.SetColor(ShaderID.VisorColor, RFSPalette.DousedPlayerNameColor);
            if (PyromaniacPlayer.Data.Role is PyromaniacRole p)
            {
                p.DousedPlayers.Add(Player);
            }
        }
    }
    
    public override void OnDeactivate()
    {
        if (Player.AmOwner)
        {
            Coroutines.Start(RFSEffects.ColorFadeAndDestroy(overlay, overlay.color, Color.clear, 0.2f));
        }

        if (PyromaniacPlayer && PyromaniacPlayer.AmOwner)
        {
            Player.cosmetics.bodySprites[0].BodySprite.material.SetColor(ShaderID.VisorColor, Palette.VisorColor);
            if (PyromaniacPlayer.Data.Role is PyromaniacRole p)
            {
                p.DousedPlayers.Add(Player);
            }
        }
    }
}