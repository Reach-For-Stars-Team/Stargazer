using UnityEngine;

namespace Stargazer.DebugTools;

public static class PathDebugVisibility
{
    public static bool Visible { get; private set; }

    public static bool TogglePressedThisFrame()
    {
        if (!Input.GetKeyDown(KeyCode.Minus) && !Input.GetKeyDown(KeyCode.KeypadMinus))
            return false;

        Visible = !Visible;
        return true;
    }
}