using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Stargazer.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class MobileBottomLeftButtonsPatch
{
    private static Transform bottomLeft;

    private static readonly Dictionary<Transform, Vector3> OriginalButtonPositions = new();

    public static void Postfix(HudManager __instance)
    {
        if (!OperatingSystem.IsAndroid())
        {
            return;
        }
        
        if (__instance == null)
        {
            return;
        }

        bottomLeft ??= FindBottomLeft(__instance);

        if (bottomLeft == null)
        {
            OriginalButtonPositions.Clear();
            return;
        }

        float offset = GetJoystickOffset();

        MoveBottomLeftButtons(offset);
    }

    private static Transform FindBottomLeft(HudManager hud)
    {
        if (!OperatingSystem.IsAndroid())
        {
            return null;
        }
        var buttons = hud.transform.Find("Buttons");

        if (buttons == null)
        {
            return null;
        }

        var found = buttons.Find("BottomLeft");

        if (found != null)
        {
            return found;
        }

        var obj = GameObject.Find("Main Camera/Hud/Buttons/BottomLeft");

        return obj != null ? obj.transform : null;
    }

    private static void MoveBottomLeftButtons(float offset)
    {
        if (!OperatingSystem.IsAndroid())
        {
            return;
        }
        var buttons = bottomLeft.GetComponentsInChildren<ActionButton>(true);

        var currentButtons = buttons
            .Where(button => button != null)
            .Select(button => button.transform)
            .ToList();

        foreach (var old in OriginalButtonPositions.Keys.ToArray())
        {
            if (old == null || !currentButtons.Contains(old))
            {
                OriginalButtonPositions.Remove(old);
            }
        }

        foreach (var buttonTransform in currentButtons)
        {
            if (!OriginalButtonPositions.ContainsKey(buttonTransform))
            {
                OriginalButtonPositions[buttonTransform] = buttonTransform.localPosition;
            }

            Vector3 original = OriginalButtonPositions[buttonTransform];

            buttonTransform.localPosition = new Vector3(
                original.x + offset,
                original.y,
                original.z
            );
        }
    }

    private static float GetJoystickOffset()
    {
        var leftVirtualJoystick = Object.FindObjectsOfType<VirtualJoystick>(true)
            .FirstOrDefault(j =>
                j != null &&
                !j.IsRightJoystick &&
                IsActiveAndVisible(j.gameObject)
            );

        if (leftVirtualJoystick != null)
        {
            float scale = Mathf.Abs(leftVirtualJoystick.transform.lossyScale.x);
            float offset = leftVirtualJoystick.OuterRadius * scale * 1.35f;

            return Mathf.Clamp(offset, 0f, 3f);
        }

        var screenJoystick = Object.FindObjectsOfType<ScreenJoystick>(true)
            .FirstOrDefault(j =>
                j != null &&
                IsActiveAndVisible(j.gameObject)
            );

        if (screenJoystick != null)
        {
            float scale = Mathf.Abs(screenJoystick.transform.lossyScale.x);
            float offset = 1.3f * scale;

            return Mathf.Clamp(offset, 0f, 3f);
        }

        return 0f;
    }

    private static bool IsActiveAndVisible(GameObject obj)
    {
        if (!OperatingSystem.IsAndroid())
        {
            return false;
        }
        
        if (obj == null || !obj.activeInHierarchy)
        {
            return false;
        }

        var renderers = obj.GetComponentsInChildren<SpriteRenderer>(true);

        if (renderers.Length == 0)
        {
            return true;
        }

        foreach (var renderer in renderers)
        {
            if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }
}