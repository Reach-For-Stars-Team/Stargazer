using System.Linq;
using AmongUs.GameOptions;
using Il2CppSystem;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Networking;
using Stargazer.Utilities;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Florist;

public class PlantButton : CustomActionButton
{
    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcSpawnFloristTrap((uint) nextType);
        SetNextType((FloristRole.FlowerTypes) UnityRandom.RandomRangeInt(0, 4));
    }

    public FloristRole.FlowerTypes nextType;

    public void SetNextType(FloristRole.FlowerTypes newType)
    {
        if (Helpers.CheckChance(25)) nextType = FloristRole.FlowerTypes.Flowers; //Weighted chance for flowers.
        nextType = newType;
        Sprite newSprite = null;
        Color outlineColor = Color.white;
        switch (nextType)
        {
            case FloristRole.FlowerTypes.Flowers:
                newSprite = Assets.PlantFlowersButton.LoadAsset();
                outlineColor = new Color32(255, 125, 255, 255);
                break;
            case FloristRole.FlowerTypes.TallGrass:
                newSprite = Assets.PlantGrassButton.LoadAsset();
                outlineColor = new Color32(159, 217, 46, 255);
                break;
            case FloristRole.FlowerTypes.Thorns:
                newSprite = Assets.PlantThornsButton.LoadAsset();
                outlineColor = new Color32(98, 161, 0, 255);
                break;
            case FloristRole.FlowerTypes.Mushroom:
                newSprite = Assets.PlantMushroomButton.LoadAsset();
                outlineColor = new Color32(120, 0, 75, 255);
                break;
            default:
                newSprite = Assets.PlantFlowersButton.LoadAsset();
                outlineColor = Color.white;
                break;
        }
        OverrideSprite(newSprite);
        SetTextOutline(outlineColor);
    }
    public override bool Enabled(RoleBehaviour role)
    {
        return role is FloristRole;
    }

    public override string Name => "Plant";

    public override float Cooldown => OptionGroupSingleton<FloristOptions>.Instance.PlantCooldown.Value;

    public override LoadableAsset<Sprite> Sprite => Assets.PlaceHolder;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        SetNextType((FloristRole.FlowerTypes) UnityRandom.RandomRangeInt(0, 3));
    }
    //private static bool HasAnyControllablePlayer()
    //{
    //    return PlayerControl.AllPlayerControls
    //        .ToArray()
    //        .Any(player =>
    //            player != PlayerControl.LocalPlayer &&
    //            player.Data != null &&
    //            !player.Data.IsDead &&
    //            player.TryGetModifier<BlossomModifier>(out BlossomModifier m) &&
    //            m.BlossomingValue >= BlossomModifier.RequiredBlossomValue
    //        );
    //}
}
public class ControlFlowerButton : CustomActionButton
{
    protected override void OnClick()
    {
        var menu = ImprovedCustomPlayerMenu.CreateImproved();

        menu.ImprovedPlayerMenu(
            player =>
                player != PlayerControl.LocalPlayer &&
                player.Data != null &&
                !player.Data.IsDead &&
                player.TryGetModifier<BlossomModifier>(out BlossomModifier m) &&
                m.BlossomingValue >= BlossomModifier.RequiredBlossomValue,
            player =>
            {
                FloristControlState.FloristPlayer = PlayerControl.LocalPlayer;
                FloristControlState.ControlledPlayer = player;

                player.RpcAddModifier<ControlledByFloristModifier>(PlayerControl.LocalPlayer);
                Timer = OptionGroupSingleton<FloristOptions>.Instance.ControlDuration.Value;
                EffectActive = true;
            },
            "#ce007b",
            ImprovedCustomPlayerMenu.EmptyIcon
            // "#ce007b",
            // Assets.JesterIcon.LoadAsset()
        );
        Timer = 0;
    }

    public override bool HasEffect => true;

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        bool shouldShow =
            (FloristControlState.ControlledPlayer != null || HasAnyControllablePlayer()) && 
            MeetingHud.Instance == null;

        Button.gameObject.SetActive(shouldShow);

        if (!shouldShow)
        {
            return;
        }

        if (FloristControlState.ControlledPlayer != null)
        {
            Button.OverrideText("Stop " + (int) Timer);
        }
        else
        {
            Button.OverrideText("Control");
            EffectActive = false;
        }
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is FloristRole;
    }

    public override bool CanUse()
    {
        if (FloristControlState.ControlledPlayer != null)
        {
            return true;
        }

        return HasAnyControllablePlayer();
    }

    private static bool HasAnyControllablePlayer()
    {
        return PlayerControl.AllPlayerControls
            .ToArray()
            .Any(player =>
                player != PlayerControl.LocalPlayer &&
                player.Data != null &&
                !player.Data.IsDead &&
                player.TryGetModifier<BlossomModifier>(out BlossomModifier m) &&
                m.BlossomingValue >= BlossomModifier.RequiredBlossomValue
            );
    }

    public override string Name => "Control";

    public override float Cooldown => OptionGroupSingleton<FloristOptions>.Instance.ControlCooldown.Value;
    
    public override LoadableAsset<Sprite> Sprite => Assets.PlaceHolder;
}
public class ControlledKillButton : CustomActionButton<PlayerControl>
{
    private PlayerControl lastOutlinedTarget;

    protected override void OnClick()
    {
        var controlled = FloristControlState.ControlledPlayer;

        if (controlled == null || controlled.Data == null || controlled.Data.IsDead)
        {
            return;
        }

        var target = GetControlledTarget();

        if (target == null || target.Data == null || target.Data.IsDead)
        {
            return;
        }

        controlled.RpcCustomMurder(
            target,
            showKillAnim: true,
            playKillSound: true,
            teleportMurderer: true
        );

        Target = null;
        ClearLastOutline();
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is FloristRole;
    }

    public override bool CanUse()
    {
        return FloristControlState.ControlledPlayer != null &&
               FloristControlState.ControlledPlayer.Data != null &&
               !FloristControlState.ControlledPlayer.Data.IsDead &&
               GetControlledTarget() != null;
    }

    public override PlayerControl GetTarget()
    {
        return GetControlledTarget();
    }

    private PlayerControl GetControlledTarget()
    {
        var controlled = FloristControlState.ControlledPlayer;

        if (controlled == null || controlled.Data == null || controlled.Data.IsDead)
        {
            return null;
        }

        return controlled.GetClosestPlayer(false, Distance);
    }

    public override void SetOutline(bool active)
    {
        if (!active)
        {
            ClearLastOutline();
            return;
        }

        var target = GetControlledTarget();

        if (target == lastOutlinedTarget)
        {
            return;
        }

        ClearLastOutline();

        if (target != null)
        {
            target.cosmetics.SetOutline(true, new Nullable<Color>(Palette.ImpostorRed));
            lastOutlinedTarget = target;
        }
    }

    private void ClearLastOutline()
    {
        if (lastOutlinedTarget != null)
        {
            lastOutlinedTarget.cosmetics.SetOutline(false, new Nullable<Color>(Palette.ImpostorRed));
            lastOutlinedTarget = null;
        }
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        
        if (Button == null)
        {
            return;
        }

        bool shouldShow = FloristControlState.ControlledPlayer != null && 
            MeetingHud.Instance == null;

        Button.gameObject.SetActive(shouldShow);

        if (!shouldShow)
        {
            Target = null;
            ClearLastOutline();
            return;
        }

        Button.OverrideText("Kill");

        Target = GetControlledTarget();
        SetOutline(Target != null);
    }

    public override string Name => "Kill";

    public override float Cooldown => 10;

    public override float Distance => 1.5f;

    public override LoadableAsset<Sprite> Sprite => Assets.PlaceHolder;
}
