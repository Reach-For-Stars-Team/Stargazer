using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Networking;
using Stargazer.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Lifesaver;

public class ReviveButton : CustomActionButton<DeadBody>
{
    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcRevive(PlayerControlUtils.GetPlayerById(Target.ParentId));
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is LifesaverRole { hasFirstAidKit: true };
    }

    public override string Name => "First Aid";

    public override float Cooldown => OptionGroupSingleton<LifesaverOptions>.Instance.AbilityCooldown.Value;

    public override LoadableAsset<Sprite> Sprite => Assets.ReviveButton;

    public override DeadBody GetTarget()
    {
        return PlayerControl.LocalPlayer.GetNearestDeadBody(Distance);
    }

    public override void SetOutline(bool active)
    {
        Target?.bodyRenderers[0].SetOutline(active ? RFSPalette.LifesaverRoleColor : Color.clear);
    }
}