using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Stargazer.Networking;
using UnityEngine;

namespace Stargazer.Roles.Neutrals.GhostBuster;

public class PlaceTrap : CustomActionButton
{
    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcPlaceGhostTrap();
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is GhostBusterRole;
    }

    public override string Name => "Place Trap";

    public override float Cooldown => 25;

    public override LoadableAsset<Sprite> Sprite => Assets.GhostTrap;
}