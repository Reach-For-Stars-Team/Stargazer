using AchievementsAPI.API;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Features;
using Stargazer.Networking;
using Stargazer.Roles.Impostors.Sleepcaster;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Cowboy;

public class Lasso : CustomActionButton
{
    private int timesUsed = 0;
    protected override void OnClick()
    {
        timesUsed++;
        AchievementsTabSingleton<StargazerAchievements>.Instance.CowboyAchievement1.Unlock();
        if (timesUsed == 3) AchievementsTabSingleton<StargazerAchievements>.Instance.CowboyAchievement2.Unlock();
        PlayerControl.LocalPlayer.transform.GetComponent<HnSImpostorScreamSfx>().LocalImpostorYeehaw();
        var target = PlayerControl.LocalPlayer.GetClosestPlayer(false, 1000f, true);
        if (target == null) return;
        PlayerControl.LocalPlayer.RpcLasso(target);
        if (target.HasModifier<SleepyModifier>()) AchievementsTabSingleton<StargazerAchievements>.Instance.CowboyAchievement3.Unlock();
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is CowboyRole;
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        timesUsed = 0;
    }

    public override string Name => "Lasso";

    public override float Cooldown => OptionGroupSingleton<CowboyOptions>.Instance.AbilityCooldown.Value;

    public override float EffectDuration => OptionGroupSingleton<CowboyOptions>.Instance.PullDuration.Value;

    public override LoadableAsset<Sprite> Sprite => Assets.LassoButton;
}