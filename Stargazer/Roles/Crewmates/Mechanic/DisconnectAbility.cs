using System.Linq;
using Il2CppSystem;
using MiraAPI.Hud; 
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using Rewired;
using Stargazer.Networking;
using UnityEngine;
using UnityEngine.Events;
using Player = RewiredConsts.Player;

namespace Stargazer.Roles.Crewmates.Mechanic;

public class DisconnectAbility : CustomActionButton<Vent>
{
    public override string Name => "Disconnect";
    public override float Cooldown => 25;
    public override float EffectDuration => 0;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => Assets.DisconnectButton;
    public override ButtonLocation Location => ButtonLocation.BottomRight;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is MechanicRole;
    }
    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.NetTransform.Halt();
        PlayerControl.LocalPlayer.moveable = false;
        Target.SetButtons(true);
        foreach (var btn in Target.Buttons)
        {
            btn.StartCoroutine(Effects.Bloop(0.1f, btn.transform, btn.transform.localScale.x, 0.5f));
            btn.spriteRenderer.color = RFSPalette.MechanicRoleColor;
            btn.OnClick.AddListener(new System.Action(() =>
            {
                if (PlayerControl.LocalPlayer.Data.Role is MechanicRole)
                {
                    PlayerControl.LocalPlayer.RpcDisconnectVent(Target.Id, Target.Buttons.IndexOf(btn));
                    Target.SetButtons(false);
                    
                    PlayerControl.LocalPlayer.moveable = true;
                }
            }));
        }
    }

    public override Vent GetTarget()
    {
        return PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(0.7f, new ContactFilter2D().NoFilter());
    }

    public override bool IsTargetValid(Vent target)
    {
        return base.IsTargetValid(target) && target.Buttons.Count(x => x.enabled) > 0;
    }

    public override void SetOutline(bool active)
    {
        Target?.myRend.SetOutline(active ? RFSPalette.MechanicRoleColor : null);
    }
}