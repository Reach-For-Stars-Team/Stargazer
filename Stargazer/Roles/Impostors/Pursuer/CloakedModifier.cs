using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Pursuer;

public class CloakedModifier : TimedModifier
{
    public override string ModifierName => "Cloaked";
    private Sprite skinBack;
    private Sprite skinFront;
    public override void OnActivate()
    {
        Player.MyPhysics.Speed *= 2;
        PlayerMaterial.SetColors(Color.black, Player.cosmetics.bodySprites[0].BodySprite);
        skinFront = Player.cosmetics.hat.FrontLayer.sprite;
        skinBack = Player.cosmetics.hat.BackLayer.sprite;
        
        Player.cosmetics.hat.FrontLayer.sprite = Assets.CloakFront.LoadAsset();
        Player.cosmetics.hat.BackLayer.sprite = Assets.CloakBack.LoadAsset();
    }

    public override void OnDeactivate()
    {
        Player.MyPhysics.Speed /= 2;
        PlayerMaterial.SetColors(Player.cosmetics.ColorId, Player.cosmetics.bodySprites[0].BodySprite);
        Player.cosmetics.hat.FrontLayer.sprite = skinFront;
        Player.cosmetics.hat.BackLayer.sprite = skinBack;
    }
    

    public override float Duration => 10;
}