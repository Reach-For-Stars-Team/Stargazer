using System.Collections;
using System.Linq;
using Internal.Cryptography;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Stargazer.Modifiers;
using Stargazer.Networking;
using Reactor.Utilities;
using Rewired;
using UnityEngine;
using Helpers = MiraAPI.Utilities.Helpers;

namespace Stargazer.Roles.Neutrals.GhostBuster;

public class Vacuum : CustomActionButton
{
    protected override void OnClick()
    {
        SoundManager.Instance.PlaySound(Assets.VacuumGhostSfx.LoadAsset(), false, 1f);
        PlayerControl.LocalPlayer.RpcVacuum();   
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is GhostBusterRole && !role.Player.HasModifier<NeutralWinner>();
    }

    public override string Name => "Vacuum";

    public override float Cooldown => 30;

    public override float EffectDuration => 1;

    public override LoadableAsset<Sprite> Sprite => Assets.VacuumButton;
}