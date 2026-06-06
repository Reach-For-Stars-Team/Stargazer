using Il2CppSystem;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Components.Minigames;
using UnityEngine;

namespace Stargazer.Roles.Neutrals.Roleless;

public class GuessAbility : CustomActionButton
{
    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.Data.Role is not RolelessRole p) return;
        
        var menu = CustomPlayerMenu.Create();
        menu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        menu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        menu.Begin(x => !x.Data.IsDead,
            player =>
            {
                menu.Close();
                RoleThiefMinigame.CreateAndOpen(player.Data.PlayerId);
            });;
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is RolelessRole;
    }

    public override string Name => "Guess";

    public override float Cooldown => 0;

    public override int MaxUses => OptionGroupSingleton<RolelessOptions>.Instance.AbilityUses;

    public override LoadableAsset<Sprite> Sprite => Assets.GuessButton;
}