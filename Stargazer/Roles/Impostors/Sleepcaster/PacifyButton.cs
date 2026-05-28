using System.Linq;
using Il2CppSystem;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Components.Tasks;
using Stargazer.Networking;
using Stargazer.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Sleepcaster;

public class PacifyButton : CustomActionButton
{
    public override string Name => "Pacify";

    public override float Cooldown => OptionGroupSingleton<SleepcasterOptions>.Instance.AbilityCooldown.Value;

    public override float EffectDuration => OptionGroupSingleton<SleepcasterOptions>.Instance.AbilityDuration.Value;

    public override LoadableAsset<Sprite> Sprite => Assets.PacifyButton;

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcPacifyPlayers();
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is SleepcasterRole;
    }
}