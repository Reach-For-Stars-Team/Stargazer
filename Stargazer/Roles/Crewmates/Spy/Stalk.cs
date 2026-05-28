using Il2CppSystem;
using MiraAPI.Hud; 
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Spy;

public class Stalk : CustomActionButton<PlayerControl>
{
    public override string Name => "Stalk";
    public override float Cooldown => 0;
    public override float EffectDuration => 0;

    public override int MaxUses => 0;

    public override LoadableAsset<Sprite> Sprite => Assets.StalkButton;
    public override ButtonLocation Location => ButtonLocation.BottomRight;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is SpyRole;
    }
    protected override void OnClick()
    {
        var watchBtn = CustomButtonSingleton<Watch>.Instance;
        watchBtn.Target = Target;
        watchBtn.Button?.Show();
        Button?.Hide();
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