using Il2CppSystem;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Networking;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Pyromaniac;

public class BurnAbility : CustomActionButton
{
    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.Data.Role is not PyromaniacRole p) return;
        
        var menu = CustomPlayerMenu.Create();
        menu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        menu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        menu.Begin(x => x.HasModifier<DousedModifier>(),
            player =>
            {
                player.RpcAddModifier<BurningModifier>(PlayerControl.LocalPlayer);
                menu.Close();
            });;
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