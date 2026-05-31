using System;
using System.Linq;
using MiraAPI.Hud;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Stargazer.Utilities;

[RegisterInIl2Cpp]
public class ImprovedCustomPlayerMenuPanel(IntPtr ptr) : MonoBehaviour(ptr)
{
    public bool UseCustomHighlightColor;
    public bool UseCustomIconSprite;
    public bool UseCustomIconColor;

    public Color HighlightColor = Color.white;
    public Color IconColor = Color.white;
    public Sprite IconSprite;

    private SpriteRenderer highlightRenderer;
    private SpriteRenderer iconRenderer;
    private Sprite originalIconSprite;
    private bool cached;

    public void Awake()
    {
        CacheRenderers();
    }

    public void LateUpdate()
    {
        ApplyChangesOnly();
    }

    private void CacheRenderers()
    {
        if (cached)
        {
            return;
        }

        var highlight = transform.Find("Nameplate/Highlight");
        if (highlight != null)
        {
            highlightRenderer = highlight.GetComponent<SpriteRenderer>();
        }

        var icon = transform.Find("Nameplate/Highlight/ShapeshifterIcon");
        if (icon != null)
        {
            iconRenderer = icon.GetComponent<SpriteRenderer>();

            if (iconRenderer != null)
            {
                originalIconSprite = iconRenderer.sprite;
            }
        }

        cached = true;
    }

    public void ApplyChangesOnly()
    {
        CacheRenderers();

        if (highlightRenderer != null && UseCustomHighlightColor)
        {
            Color current = highlightRenderer.color;

            highlightRenderer.color = new Color(
                HighlightColor.r,
                HighlightColor.g,
                HighlightColor.b,
                current.a
            );
        }

        if (iconRenderer != null)
        {
            if (UseCustomIconColor)
            {
                Color current = iconRenderer.color;

                iconRenderer.color = new Color(
                    IconColor.r,
                    IconColor.g,
                    IconColor.b,
                    current.a
                );
            }

            if (UseCustomIconSprite && IconSprite != null)
            {
                iconRenderer.sprite = IconSprite;
            }
        }
    }

    public void Restore()
    {
        CacheRenderers();

        if (iconRenderer != null && originalIconSprite != null)
        {
            iconRenderer.sprite = originalIconSprite;
        }
    }
}

public static class ImprovedCustomPlayerMenu
{
    public static CustomPlayerMenu CreateImproved()
    {
        var menu = CustomPlayerMenu.Create();
        menu.SetLocalPlayerMaterial();
        return menu;
    }

    public static void SetLocalPlayerMaterial(this CustomPlayerMenu menu)
    {
        if (menu == null || PlayerControl.LocalPlayer == null)
        {
            return;
        }

        var material = PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        var phoneUi = menu.transform.FindChild("PhoneUI");
        if (phoneUi == null)
        {
            return;
        }

        var first = phoneUi.GetChild(0)?.GetComponent<SpriteRenderer>();
        if (first != null)
        {
            first.material = material;
        }

        var second = phoneUi.GetChild(1)?.GetComponent<SpriteRenderer>();
        if (second != null)
        {
            second.material = material;
        }
    }

    public static void ImprovedPlayerMenu(
        this CustomPlayerMenu menu,
        System.Func<PlayerControl, bool> playerMatch,
        Action<PlayerControl> onClick
    )
    {
        menu.ImprovedPlayerMenu(playerMatch, onClick, (Color?)null, null);
    }

    public static void ImprovedPlayerMenu(
        this CustomPlayerMenu menu,
        System.Func<PlayerControl, bool> playerMatch,
        Action<PlayerControl> onClick,
        string highlightHex,
        Sprite iconSprite = null
    )
    {
        Color? highlightColor = null;

        if (!string.IsNullOrEmpty(highlightHex) &&
            ColorUtility.TryParseHtmlString(highlightHex, out Color parsedColor))
        {
            highlightColor = parsedColor;
        }

        menu.ImprovedPlayerMenu(playerMatch, onClick, highlightColor, iconSprite);
    }

    public static void ImprovedPlayerMenu(
        this CustomPlayerMenu menu,
        System.Func<PlayerControl, bool> playerMatch,
        Action<PlayerControl> onClick,
        Color? highlightColor = null,
        Sprite iconSprite = null
    )
    {
        if (menu == null)
        {
            return;
        }

        menu.Begin(
            playerMatch,
            player =>
            {
                if (player != null)
                {
                    onClick?.Invoke(player);
                }

                menu.Close();
            }
        );

        Apply(menu, playerMatch, highlightColor, iconSprite);
    }

    public static void Apply(
        CustomPlayerMenu menu,
        System.Func<PlayerControl, bool> playerMatch,
        Color? highlightColor = null,
        Sprite iconSprite = null
    )
    {
        if (menu == null || menu.potentialVictims == null)
        {
            return;
        }

        if (!highlightColor.HasValue && iconSprite == null)
        {
            return;
        }

        var players = PlayerControl.AllPlayerControls
            .ToArray()
            .Where(playerMatch)
            .ToList();

        for (int i = 0; i < menu.potentialVictims.Count && i < players.Count; i++)
        {
            var panel = menu.potentialVictims[i];

            var style = panel.gameObject.GetComponent<ImprovedCustomPlayerMenuPanel>();

            if (style == null)
            {
                style = panel.gameObject.AddComponent<ImprovedCustomPlayerMenuPanel>();
            }

            style.UseCustomHighlightColor = highlightColor.HasValue;
            style.HighlightColor = highlightColor ?? Color.white;

            style.UseCustomIconSprite = iconSprite != null;
            style.IconSprite = iconSprite;

            style.UseCustomIconColor = false;
            style.IconColor = Color.white;

            style.ApplyChangesOnly();
        }
    }
    private static Sprite emptyIcon;

    public static Sprite EmptyIcon
    {
        get
        {
            if (emptyIcon != null)
            {
                return emptyIcon;
            }

            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, new Color(1f, 1f, 1f, 0f));
            tex.Apply();

            emptyIcon = Sprite.Create(
                tex,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                100f
            );

            return emptyIcon;
        }
    }
}