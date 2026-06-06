using System;
using System.Collections.Generic;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.LocalSettings;
using MiraAPI.Roles;
using Reactor.Utilities.Attributes;
using Stargazer.Options;
using UnityEngine;
using Action = Il2CppSystem.Action;

namespace Stargazer.Components;

[RegisterInIl2Cpp]
public class RoleIconBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl myPlayer;
    public SpriteRenderer myRenderer;
    public static Dictionary<PlayerControl, RoleIconBehaviour> RoleIcons = new Dictionary<PlayerControl, RoleIconBehaviour>();
    public void FixedUpdate()
    {
        if (myPlayer == null || myRenderer == null) return;
        
        myRenderer.enabled = CanLocalPlayerSeeRole();
    }

    private void Start()
    {
        if (myPlayer == null) return;
        RoleIcons.Add(myPlayer, this);
    }

    private bool CanLocalPlayerSeeRole()
    {
        return LocalSettingsTabSingleton<ClientOptions>.Instance.EnableRoleIcons.Value &&
            myPlayer.Visible 
            && (myPlayer.AmOwner || PlayerControl.LocalPlayer.Data.IsDead || (myPlayer.Data.Role.IsImpostor && PlayerControl.LocalPlayer.Data.Role.IsImpostor) || (myPlayer.Data.Role is ICustomRole c && c.CanLocalPlayerSeeRole(PlayerControl.LocalPlayer)));
    }

    public static RoleIconBehaviour GetRoleIconBehaviour(PlayerControl player)
    {
        return RoleIcons.GetValueOrDefault(player);
    }

    [RegisterEvent]
public static void OnSetRole(SetRoleEvent e)
{
    e.Player.StartCoroutine(Effects.ActionAfterDelay(0.01f, new System.Action(() =>
    {
        var icon = GetRoleIconBehaviour(e.Player);
        if (icon == null) return;

        GetRoleIcon(e.Player.Data.Role, out Sprite sprite);
        icon.myRenderer.sprite = sprite;
        if (sprite != null)
            icon.transform.localScale = GetRoleIconScale(sprite);
    })));
}
    
    public static bool GetRoleIcon(RoleBehaviour role, out Sprite sprite)
    {
        if (role is ICustomRole custom)
        {
            sprite = custom.Configuration.Icon?.LoadAsset();
            return sprite != null;
        }

        sprite = role.RoleIconSolid;
        return sprite != null;
    }
    
    private static Vector3 GetRoleIconScale(Sprite icon)
    {
        float spritePixelHeight = icon.rect.height;
        float ppu = icon.pixelsPerUnit;
        float currentWorldHeight = spritePixelHeight / ppu;
        float desiredWorldHeight = 0.4f;
        float scale = desiredWorldHeight / currentWorldHeight;
        return new(scale, scale, 1);
    }
}