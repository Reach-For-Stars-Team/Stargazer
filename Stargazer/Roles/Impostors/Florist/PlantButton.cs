using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Stargazer.Networking;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Florist;

public class PlantButton : CustomActionButton
{
    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcSpawnFloristTrap(1);
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is FloristRole;
    }

    public override string Name => "Plant";

    public override float Cooldown => 5;

    public override LoadableAsset<Sprite> Sprite => Assets.PlaceHolder;
}