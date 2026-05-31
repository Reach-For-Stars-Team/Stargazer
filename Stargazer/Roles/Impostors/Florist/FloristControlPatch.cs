using HarmonyLib;
using Rewired;
using Stargazer.Networking;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Florist;

[HarmonyPatch]
public static class FloristControlPatches
{
    private static Vector2 lastSentVelocity;
    private static float sendTimer;

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    [HarmonyPrefix]
    public static bool PlayerPhysicsFixedUpdatePrefix(PlayerPhysics __instance)
    {
        if (__instance == null || __instance.myPlayer == null)
        {
            return true;
        }

        var controlled = FloristControlState.ControlledPlayer;
        var florist = FloristControlState.FloristPlayer;

        if (controlled == null)
        {
            return true;
        }

        if (__instance.myPlayer == controlled)
        {
            __instance.HandleAnimation(__instance.myPlayer.Data.IsDead);
            return false;
        }

        if (florist != null && __instance.myPlayer == florist && florist.AmOwner)
        {
            __instance.body.velocity = Vector2.zero;
            __instance.HandleAnimation(__instance.myPlayer.Data.IsDead);
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePostfix()
    {
        var local = PlayerControl.LocalPlayer;

        if (local == null || local.Data == null || local.Data.IsDead)
        {
            return;
        }

        if (local.Data.Role is not FloristRole)
        {
            return;
        }

        var controlled = FloristControlState.ControlledPlayer;

        if (controlled == null || controlled.Data == null || controlled.Data.IsDead)
        {
            return;
        }

        if (FloristControlState.FloristPlayer != local)
        {
            return;
        }

        Vector2 input = GetInputVector();

        float speed = controlled.MyPhysics.TrueSpeed;
        Vector2 velocity = input * speed;

        controlled.MyPhysics.body.velocity = velocity;
        controlled.MyPhysics.HandleAnimation(controlled.Data.IsDead);

        sendTimer += Time.deltaTime;

        if (sendTimer >= 0.05f || velocity != lastSentVelocity)
        {
            sendTimer = 0f;
            lastSentVelocity = velocity;

            local.RpcMoveFloristControlledPlayer(
                controlled.PlayerId,
                velocity.x,
                velocity.y
            );
        }
    }

    private static Vector2 GetInputVector()
    {
        Vector2 input = Vector2.zero;

        if (DestroyableSingleton<HudManager>.InstanceExists)
        {
            var joystick = DestroyableSingleton<HudManager>.Instance.joystick;

            if (joystick != null)
            {
                input = joystick.DeltaL;
            }
        }

        if (input == Vector2.zero)
        {
            input = GetRewiredInput();
        }

        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        return input;
    }

    private static Vector2 GetRewiredInput()
    {
        Vector2 input = Vector2.zero;

        var player = ReInput.players.GetPlayer(0);

        if (player == null)
        {
            return input;
        }

        if (player.GetButton(40))
        {
            input.x += 1f;
        }

        if (player.GetButton(39))
        {
            input.x -= 1f;
        }

        if (player.GetButton(44))
        {
            input.y += 1f;
        }

        if (player.GetButton(42))
        {
            input.y -= 1f;
        }

        return input;
    }
}