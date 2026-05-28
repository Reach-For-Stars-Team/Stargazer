using Il2CppSystem;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using Stargazer.Utilities;
using UnityEngine;

namespace Stargazer.Roles.Neutrals.Pirate;

public class DigAbility : CustomActionButton
{
    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.Data.Role is PirateRole p)
        {
            p.IncreaseGold(OptionGroupSingleton<PirateOptions>.Instance.GoldPerTreasure);
            Button?.StartCoroutine(Effects.ScaleIn(p.xMark.transform, 1, 0, 0.6f));
            Coroutines.Start(RFSEffects.ColorFadeAndDestroy(p.xMark, Color.white, Color.white.ToClearColor(), 0.7f));
            var chest = UnityObject.Instantiate(Assets.ChestPopoutPrefab.LoadAsset());
            chest.layer = LayerMask.NameToLayer("Objects");
            chest.transform.position = PlayerControl.LocalPlayer.transform.position;
        }
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is PirateRole;
    }

    public override string Name => "Dig";

    public override float Cooldown => 20;
    
    public override LoadableAsset<Sprite> Sprite => Assets.PirateDigButton;

    public override bool CanUse()
    {
        return PlayerControl.LocalPlayer.Data.Role is PirateRole p && p.xMark && Vector2.Distance(p.xMark.transform.position, PlayerControl.LocalPlayer.transform.position) < 1f;
    }
}