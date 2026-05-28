using MiraAPI.Colors;
using UnityEngine;

namespace Stargazer;

[RegisterCustomColors]
public static class Colors
{
    public static CustomColor Cerulean { get; } = new("Cerulean", new Color(0.0f, 0.48f, 0.65f));

    public static CustomColor Fortegreen { get; } = new("Fortegreen", new Color(0.1f, 0.59f, 0.32f));
    
    public static CustomColor Cherry { get; } = new("Cherry", new Color(1.0f, 0.61f, 0.83f));

    public static CustomColor Magenta { get; } = new("Magenta", new Color(0.96f, 0.05f, 0.55f));

    public static CustomColor Olive { get; } = new("Olive", new Color(0.38f, 0.44f, 0.09f));

    public static CustomColor Dark { get; } = new("Dark", new Color(0f, 0f, 0f));

    public static CustomColor Beige { get; } = new("Beige", new Color(1f, 0.74f, 0.64f));

    public static CustomColor Mint { get; } = new("Mint", new Color(0.63f, 0.9f, 0.72f));

    public static CustomColor DarkRed { get; } = new("dark Red", new Color(0.5f, 0.01f, 0f));

    public static CustomColor DarkPurple { get; } = new("Dark Purple", new Color(0.6f, 0.11f, 0.6f));

    public static CustomColor Salmon { get; } = new("Salmon", new Color(0.98f, 0.51f, 0.45f));

    public static CustomColor Mauveine { get; } = new("Mauveine", new Color(0.56f, 0.36f, 0.51f));

    public static CustomColor Lavender { get; } = new("Lavender", new Color(0.87f, 0.6f, 1f));

    public static CustomColor Colorless { get; } = new("Colorless", new Color(1f, 0f, 0f), new Color(0f, 0f, 1f, 1f));

    public static CustomColor Gold { get; } = new("Gold", new Color(1f, 1f, 0.05f), new Color(1f, 0f, 0f, 1f));

    public static CustomColor Sky { get; } =
        new("Soul", new Color32(159, 201, 235, 255), new Color32(93, 163, 235, 255));
    
    public static CustomColor Sunset { get; } =
        new("Sunset", new Color32(200, 125, 60, 255), new Color32(93, 163, 235, 255));
}