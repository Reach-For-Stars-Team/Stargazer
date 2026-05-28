using Il2CppSystem;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Stargazer.Roles.Neutrals.Pirate;

public class StealAbility : CustomActionButton<PlayerControl>
{
    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.Data.Role is not PirateRole pirate) return;
        pirate.IncreaseGold(OptionGroupSingleton<PirateOptions>.Instance.GoldPerSteal);
        if (Helpers.CheckChance(OptionGroupSingleton<PirateOptions>.Instance.MurderChanceWhenStealing))
        {
            PlayerControl.LocalPlayer.RpcCustomMurder(Target, playKillSound:false);
        }
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is PirateRole;
    }

    public override string Name => "Steal";

    public override float Cooldown => 20;

    public override LoadableAsset<Sprite> Sprite => Assets.StealButton;

    public override PlayerControl GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active,new Nullable<Color>(RFSPalette.PirateRoleColor));
    }
}