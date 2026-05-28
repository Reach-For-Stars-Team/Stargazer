using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using TMPro;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Spy;

public class BeingSpiedOnModifier(PlayerControl Source) : GameModifier
{
    public override string ModifierName => "Spied On, just like the new UK 'child safety' act";
    public override int GetAssignmentChance() => 0;
    public override int GetAmountPerGame() => 0;
    public PlayerControl Snoop = Source;
    public Camera Cam;
    public TextMeshPro Text;
    public override void OnActivate()
    {
        if (Player == PlayerControl.LocalPlayer)
        {
            Text = Helpers.CreateTextLabel("SpiedOnText", HudManager.Instance.transform,
                AspectPosition.EdgeAlignments.Top, new(0, 1, 0));

            Text.text = "You're being watched.";
        }

        if (Snoop == PlayerControl.LocalPlayer)
        {
            Cam = new GameObject("StalkerCamera").AddComponent<Camera>();
            Cam.transform.parent = Player.transform;
            Cam.targetTexture = Assets.StalkCamTex.LoadAsset();
            Cam.orthographic = true;
            Cam.orthographicSize = 5;
            Cam.backgroundColor = Color.black;
            Cam.transform.localPosition = new(0, 0, -1);
        }
    }
}