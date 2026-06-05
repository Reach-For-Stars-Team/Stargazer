using System.Reflection;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using Stargazer.Networking;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Florist;

public static class FloristControlState
{
    public static PlayerControl FloristPlayer;
    public static PlayerControl ControlledPlayer;

    public static MonoBehaviour PreviousCameraTarget;
}

public class ControlledByFloristModifier : TimedModifier
{
    public override string ModifierName => "Controlled By Florist";

    public override bool HideOnUi => true;
    public Sprite hatSprite;

    public PlayerControl FloristPlayer;
    
    private GameObject overlay;

    public ControlledByFloristModifier(PlayerControl source)
    {
        FloristPlayer = source;
    }

    public override void OnActivate()
    {
        FloristControlState.FloristPlayer = FloristPlayer;
        FloristControlState.ControlledPlayer = Player;
        
        hatSprite = Player.cosmetics.hat.FrontLayer.sprite;
        Player.cosmetics.hat.FrontLayer.sprite = Assets.FlowerTakeoverSprite.LoadAsset();

        if (Player.AmOwner)
        {
            Player.moveable = false;
            Player.NetTransform.Halt();
            Player.MyPhysics.body.velocity = Vector2.zero;
            overlay = UnityObject.Instantiate(Assets.ControlledOverlay.LoadAsset());
        }

        if (FloristPlayer != null && FloristPlayer.AmOwner)
        {
            // FloristPlayer.moveable = false;
            FloristPlayer.NetTransform.Halt();
            FloristPlayer.MyPhysics.body.velocity = Vector2.zero;
            overlay = UnityObject.Instantiate(Assets.ControlledOverlay.LoadAsset());

            var follower = Camera.main.GetComponent<FollowerCamera>();
            if (follower != null)
            {
                FloristControlState.PreviousCameraTarget = follower.Target;
                follower.SetTarget(Player);
            }
        }
    }
    public override float Duration => OptionGroupSingleton<FloristOptions>.Instance.ControlDuration.Value;
    public override void OnDeactivate()
    {
        Player.cosmetics.hat.FrontLayer.sprite = hatSprite;
        if (overlay) overlay.Destroy();
        if (Player.AmOwner)
        {
            Player.moveable = true;
            Player.MyPhysics.body.velocity = Vector2.zero;
        }

        if (FloristPlayer != null && FloristPlayer.AmOwner)
        {
            FloristPlayer.moveable = true;
            FloristPlayer.MyPhysics.body.velocity = Vector2.zero;

            var follower = Camera.main.GetComponent<FollowerCamera>();
            if (follower != null)
            {
                if (FloristControlState.PreviousCameraTarget != null)
                {
                    follower.SetTarget(FloristControlState.PreviousCameraTarget);
                }
                else
                {
                    follower.SetTarget(FloristPlayer);
                }
            }
        }

        if (Player.TryGetModifier(out BlossomModifier blossom))
        {
            if (OptionGroupSingleton<FloristOptions>.Instance.ResetBlossomAfterControl.Value) blossom.ResetBlossom();
        }

        if (FloristControlState.ControlledPlayer == Player)
        {
            FloristControlState.ControlledPlayer = null;
            FloristControlState.FloristPlayer = null;
            FloristControlState.PreviousCameraTarget = null;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        //if (FloristControlState.FloristPlayer.AmOwner)0000000000000000000000
        
        //{
        //    var target = Player.GetClosestPlayer(false, 1.5f); //TODO Replace with kill distance opt...
        //    HudManager.Instance.KillButton.SetTarget(target);
        //}
        //this will have to wait for xaine's pr to be merged... cool.
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class FloristControlLightPatch
{
    private static Transform cachedLight;
    private static Vector3 originalLocalPosition;
    private static Quaternion originalLocalRotation;
    private static Vector3 originalLocalScale;
    private static bool hasOriginalTransform;

    public static void Postfix(PlayerControl __instance)
    {
        try
        {
            if (__instance == null)
            {
                return;
            }

            if (PlayerControl.LocalPlayer == null)
            {
                return;
            }

            if (__instance != PlayerControl.LocalPlayer)
            {
                return;
            }

            if (__instance.Data == null || __instance.Data.IsDead)
            {
                RestoreLight();
                return;
            }

            var controlled = FloristControlState.ControlledPlayer;

            if (controlled == null || controlled.Data == null || controlled.Data.IsDead)
            {
                RestoreLight();
                return;
            }

            if (FloristControlState.FloristPlayer == null || FloristControlState.FloristPlayer != __instance)
            {
                RestoreLight();
                return;
            }

            if (cachedLight == null)
            {
                cachedLight = FindPlayerLight(__instance);

                if (cachedLight != null && !hasOriginalTransform)
                {
                    originalLocalPosition = cachedLight.localPosition;
                    originalLocalRotation = cachedLight.localRotation;
                    originalLocalScale = cachedLight.localScale;
                    hasOriginalTransform = true;
                }
            }

            if (cachedLight == null)
            {
                return;
            }

            cachedLight.position = new Vector3(
                controlled.transform.position.x,
                controlled.transform.position.y,
                controlled.transform.position.y / 1000f
            );
        }
        catch
        {
            RestoreLight();
        }
    }

    public static void RestoreLight()
    {
        if (cachedLight != null && hasOriginalTransform)
        {
            cachedLight.localPosition = originalLocalPosition;
            cachedLight.localRotation = originalLocalRotation;
            cachedLight.localScale = originalLocalScale;
        }

        cachedLight = null;
        hasOriginalTransform = false;
    }

    private static Transform FindPlayerLight(PlayerControl player)
    {
        if (player == null)
        {
            return null;
        }

        var direct = player.transform.Find("Light(Clone)");

        if (direct != null)
        {
            return direct;
        }

        var transforms = player.GetComponentsInChildren<Transform>(true);

        foreach (var t in transforms)
        {
            if (t != null && t.name == "Light(Clone)")
            {
                return t;
            }
        }

        return null;
    }
}