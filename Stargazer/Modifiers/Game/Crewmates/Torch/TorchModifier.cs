using System;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Map;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using Stargazer.Utilities;
using UnityEngine;

namespace Stargazer.Modifiers.Game.Crewmates.Torch;

public class TorchModifier : GameModifier
{
    public override string ModifierName => "Torch";

    public override string GetDescription()
    {
        return "You can use a flashlight while lights are off.";
    }

    public override int GetAssignmentChance()
    {
        return OptionGroupSingleton<TorchModifierOptions>.Instance.AssignmentChance;
    }

    public override int GetAmountPerGame()
    {
        return OptionGroupSingleton<TorchModifierOptions>.Instance.AmountPerGame;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.TeamType == RoleTeamTypes.Crewmate;
    }

    private SpriteRenderer torch;
    public override void OnActivate()
    {
        torch = new GameObject("Hand").AddComponent<SpriteRenderer>();
        torch.transform.parent = Player.transform;
        torch.transform.localPosition = new(0, 0, -50);
        torch.transform.localScale = new Vector3(.7f, .7f, 1f);
        
        torch.sprite = Assets.HandHoldingTorch.LoadAsset();
        torch.material = new Material(Shader.Find("Unlit/PlayerShader"));
        PlayerMaterial.SetColors(Player.cosmetics.ColorId, torch);
        
        torch.gameObject.SetActive(false);
    }

    [RegisterEvent]
    public static void OnSystemUpdate(UpdateSystemEvent e)
    {
        if (e.SystemType is not SystemTypes.Electrical) return;
        PlayerControl.LocalPlayer.TryGetModifier(out TorchModifier mod);

        if (mod != null)
        {
            mod.UpdateTorch();
        }
    }

    public bool SabotageActive;

    public void UpdateTorch()
    {
        Player.StartCoroutine(Effects.ActionAfterDelay(5f, new Action(() =>
        {
            SabotageActive = !SabotageActive;
            Player.lightSource.flashlightSize =
                OptionGroupSingleton<TorchModifierOptions>.Instance.FlashlightSize.Value * 3;
            Player.lightSource.SetFlashlightEnabled(SabotageActive);
            Player.StartCoroutine(Effects.Slide2D(torch.transform, new(0f, -1), Vector2.zero, 0.1f));
        })));
    }

    public override void FixedUpdate()
    {
        torch.flipX = Player.MyPhysics.FlipX;
        torch.gameObject.SetActive(SabotageActive && Player.Visible);
    }
}