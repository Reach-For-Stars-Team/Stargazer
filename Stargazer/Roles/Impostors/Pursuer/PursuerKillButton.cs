using Il2CppSystem;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Pursuer;

public class PursuerKillButton : MultiTargetButton<PlayerControl>
{
    protected override void OnClick()
    {
        if (Targets.Length == 1)
        {
            PlayerControl.LocalPlayer.RpcCustomMurder(Targets[0]);
            PlayerControl.LocalPlayer.RpcAddModifier<CloakedModifier>();
        }
        else
        {
            foreach (var target in Targets)
            {
                PlayerControl.LocalPlayer.RpcCustomMurder(target);
            }
        }
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is PursuerRole;
    }

    public override string Name => "Kill";

    public override float Cooldown => 45;

    public override LoadableAsset<Sprite> Sprite => Assets.PlaceHolder;

    public override PlayerControl[] GetTargets()
    {
        if (PlayerControl.LocalPlayer.Data.Role is not PursuerRole p) return [];

        var nearbyPlayers = Helpers.GetClosestPlayers(PlayerControl.LocalPlayer, Distance * 4, false).ToArray();
        var nearestPlayer = PlayerControl.LocalPlayer.GetClosestPlayer(false, Distance);
        switch (p.Mode)
        {
            case PursuerRole.PursuitMode.Single:
                return nearbyPlayers.Length == 1 ? [ nearestPlayer ] : [];
            case PursuerRole.PursuitMode.Multiple:
                return nearbyPlayers;
            default:
                return [];
        }
    }

    public override void SetOutline(PlayerControl target, bool active)
    {
        target.cosmetics.SetOutline(active, new Nullable<Color>(Color.red));
    }
}