using UnityEngine;

namespace Stargazer.Utilities;

public static class ColorUtils
{
    public static Color ToClearColor(this Color color)
    {
        return new Color(color.r, color.g, color.b, 0);
    }
}