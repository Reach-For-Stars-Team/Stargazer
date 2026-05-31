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
        PlayerControl.LocalPlayer.RpcSpawnFloristTrap(1);
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is FloristRole;
    }

    public override string Name => "Plant";

    public override float Cooldown => OptionGroupSingleton<FloristOptions>.Instance.PlantCooldown.Value;

    public override LoadableAsset<Sprite> Sprite => Assets.PlaceHolder;

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
}
public class ControlFlowerButton : CustomActionButton
{
    private bool resetTimerAfterOpeningMenu;
    protected override void OnClick()
    {
        if (FloristControlState.ControlledPlayer != null)
        {
            PlayerControl.LocalPlayer.RpcStopFloristControl(FloristControlState.ControlledPlayer.PlayerId);
            return;
        }
        resetTimerAfterOpeningMenu = true;
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
                FloristControlState.ControlTimer = FloristControlState.ControlDuration;

                player.RpcAddModifier<ControlledByFloristModifier>(PlayerControl.LocalPlayer);
            },
            "#ce007b",
            ImprovedCustomPlayerMenu.EmptyIcon
            // "#ce007b",
            // Assets.JesterIcon.LoadAsset()
        );
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (FloristControlState.ShouldStartControlCooldown)
        {
            FloristControlState.ShouldStartControlCooldown = false;
            Timer = OptionGroupSingleton<FloristOptions>.Instance.ControlCooldown.Value;
        }

        if (resetTimerAfterOpeningMenu)
        {
            resetTimerAfterOpeningMenu = false;
            Timer = 0f;
        }

        if (Button == null)
        {
            return;
        }

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
            Button.OverrideText("Stop " + Mathf.CeilToInt(FloristControlState.ControlTimer));
        }
        else
        {
            Button.OverrideText("Control");
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
    // public override float Cooldown => 0;

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
