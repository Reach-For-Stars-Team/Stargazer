using System;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Utilities;
using Stargazer.Utilities.Buttons;
using Reactor.Utilities.Extensions;
using Rewired;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Phaser;

public class PhaseButton : TargetedPositionActionButton
{
    public override void OnSelectTargetPosition(Vector2 position)
    {
        Button.StartCoroutine(Effects.All([
            Effects.Wait(0.5f),
            Effects.PulseColor(PlayerControl.LocalPlayer.cosmetics.bodySprites[0].BodySprite, Color.white, Color.magenta, 0.7f), 
            Effects.Wait(0.3f),
            Effects.Action(new Action(() =>
            {
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
            })),
            Effects.Bloop(0.2f, PlayerControl.LocalPlayer.transform, PlayerControl.LocalPlayer.transform.localScale.x, 0.6f)
        ]));

        SoundManager.Instance.PlaySound(Assets.TeleportSfx.LoadAsset(), false);
    }

    public override bool IsTargetValid(Vector2 pos)
    {
        return Helpers.GetRoom(pos);
    }

    protected override void OnClick()
    {
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is PhaserRole;
    }

    public override string Name => "Phase";
    
    public override float Cooldown => 60;

    public override LoadableAsset<Sprite> TargetSprite => Assets.PhaseTarget;

    public override LoadableAsset<Sprite> Sprite => Assets.WarpButton;
}