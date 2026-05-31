using Cpp2IL.Core.Utils;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using Stargazer.Roles.Impostors.Mastermind;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Impostors;

public class RecruitedKill : CustomActionButton<PlayerControl>
{
    public override string Name => "Kill";
    public override Color TextOutlineColor => Palette.ImpostorRoleRed;
    
    public override float Cooldown => OptionGroupSingleton<MastermindOptions>.Instance.RecruitedKillCooldown.Value;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.KeybindButton;
    

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(false, OptionGroupSingleton<MastermindOptions>.Instance.RecruitedKillRange.Value);
    }

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
    }
    public override bool Enabled(RoleBehaviour role)
    {
        return PlayerControl.LocalPlayer &&
               PlayerControl.LocalPlayer.HasModifier<RecruitedModifier>() &&
               !PlayerControl.LocalPlayer.Data.IsDead;
    }
    
    public override void SetOutline(bool active)
    {
    }
    
}