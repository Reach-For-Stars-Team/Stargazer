using MiraAPI.Modifiers;
using TMPro;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Florist;

public class BlossomModifier : BaseModifier
{
    public override string ModifierName => "Blossom";

    public override bool HideOnUi => true;

    public int BlossomingValue = 0;
    private TextMeshPro BlossomText;
    public override void OnActivate()
    {
        BlossomText = UnityObject.Instantiate(Player.cosmetics.nameText, Player.cosmetics.nameTextContainer.transform);
        BlossomText.transform.position = Vector3.up;
        BlossomText.text = "";
    }

    public void IncreaseBlossom()
    {
        BlossomingValue++;
        BlossomText.text = "";
        for (int i = 30; i >= BlossomingValue; i += 30)
        {
            BlossomText.text += "✿";
        }
    }
}