using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Networking;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Lightener;

public class LightUp : CustomActionButton
{
    public override string Name { get; } = "Light up";
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override float Cooldown => OptionGroupSingleton<LightenerOptions>.Instance.LightenUpCD.Value;
    public override int MaxUses => OptionGroupSingleton<LightenerOptions>.Instance.LightenUpUses;
    public override LoadableAsset<Sprite> Sprite { get; } = Assets.LightUpButton;

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcPlaceLantern();
    }

    public override bool CanUse()
    {
        return PlayerControl.LocalPlayer.GetNearestObjectOfType<Collider>(LayerMask.NameToLayer("Ship"),
                   new ContactFilter2D().NoFilter()) is null &&
               base.CanUse();
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is LightenerRole;
    }
}