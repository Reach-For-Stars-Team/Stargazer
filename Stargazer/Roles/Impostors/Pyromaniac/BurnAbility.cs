using Il2CppSystem;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Networking;
using Stargazer.Utilities;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Pyromaniac;

public class BurnAbility : CustomActionButton
{
    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.Data.Role is not PyromaniacRole) return;

        var menu = ImprovedCustomPlayerMenu.CreateImproved();

        menu.ImprovedPlayerMenu(
            player => player.HasModifier<DousedModifier>(),
            player => player.RpcAddModifier<BurningModifier>(PlayerControl.LocalPlayer),
            "#f14838",
            Assets.PyromaniacRoleIcon.LoadAsset()
        );
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is PyromaniacRole;
    }

    private Animator animator;
    
    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        animator = Button.graphic.gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = Assets.BurnButtonAnimationController.LoadAsset();
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        animator.enabled = CanUse();
    }

    public override string Name => "Burn";

    public override float Cooldown => 0;

    public override LoadableAsset<Sprite> Sprite => Assets.PlaceHolder;
}
