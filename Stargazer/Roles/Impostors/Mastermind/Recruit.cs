using System.Linq;
using JetBrains.Annotations;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using PowerTools;
using Rewired;
using Stargazer.Networking;
using Stargazer.Roles.Impostors.Mastermind;
using Stargazer.Roles.Neutrals;
using Stargazer.Roles.Neutrals.Pirate;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Mastermind;

public class Recruit : CustomActionButton<PlayerControl>
{
    protected override void OnClick()
    {
        if (Target.Data.Role is INeutralRole)
        {
            PlayerControl.LocalPlayer.RpcCustomMurder(PlayerControl.LocalPlayer);
            return;
        }
        
        PlayerControl.LocalPlayer.RpcBeginRecruiting(Target);
    }
    public override bool Enabled(RoleBehaviour role)
    {
        return role is MastermindRole;
    }
    public override string Name => "Recruit";
    public override float Cooldown => OptionGroupSingleton<MastermindOptions>.Instance.RecruitCooldown.Value;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override LoadableAsset<Sprite> Sprite => Assets.RecruitButton;
    public override void SetOutline(bool active)
    {
    }
    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(false, OptionGroupSingleton<MastermindOptions>.Instance.RecruitRange.Value);
    }
}