using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Stargazer.Networking;
using Stargazer.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Paranoiac;

public class ParanoidModifier : BaseModifier
{
    public override string ModifierName => "Paranoid";
    public void ShowIndicator(PlayerControl source)
    {
        var go = new GameObject("AbilityUsedIndicator");
        go.transform.position = source.transform.position;
        var rend = go.AddComponent<SpriteRenderer>();
        rend.sprite = Assets.AbilityUsedVisual.LoadAsset();
        Player.StartCoroutine(Effects.ScaleIn(go.transform, 0, 1, 2));
        Coroutines.Start(RFSEffects.ColorFadeAndDestroy(rend, RFSPalette.ParanoiacRoleColor, Color.clear, 2.5f));
    }

    public override void OnActivate()
    {
        if (!Player.AmOwner) return;

        var opt = OptionGroupSingleton<ParanoiacOptions>.Instance;
        HudManager.Instance.PlayerCam.ShakeScreen(opt.AbilityDuration.Value, 1);
    }

    [RegisterEvent]
    public static void OnUseMiraAbility(MiraButtonClickEvent e)
    {
        if (e.Button.CanUse()) RPCHandler.RpcUseAbility(PlayerControl.LocalPlayer);
    }
    
    [RegisterEvent]
    public static void OnUseVanillaAbility(VanillaButtonClickEvent e)
    {
        if (!e.Button.IsOnCooldown) RPCHandler.RpcUseAbility(PlayerControl.LocalPlayer);
    }
}