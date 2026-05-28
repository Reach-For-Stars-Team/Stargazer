using Il2CppSystem;
using MiraAPI.Hud; 
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Networking;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Actor;

public class Act : CustomActionButton<PlayerControl>
{
    public override string Name => "Act";
    public override float Cooldown => 25;
    public override float EffectDuration => 0;

    public override int MaxUses => 0;

    public override LoadableAsset<Sprite> Sprite => Assets.ActButton;
    public override ButtonLocation Location => ButtonLocation.BottomRight;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is ActorRole;
    }
    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcAct(Target, UnityRandom.RandomRange(25, 45));
    }

    public override PlayerControl GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Nullable<Color>(Palette.CrewmateBlue));
    }
}