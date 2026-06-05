using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Florist;

public class BlossomModifier : BaseModifier
{
    public override string ModifierName => "Blossom";

    public override bool HideOnUi => true;

    public int BlossomingValue = 0;

    private float timer;

    public static int RequiredBlossomValue => (int)OptionGroupSingleton<FloristOptions>.Instance.BlossomTime.Value;

    public void IncreaseBlossom()
    {
        timer += Time.fixedDeltaTime;

        if (timer < 1f)
        {
            return;
        }

        timer = 0f;
        BlossomingValue++;

        if (BlossomingValue > RequiredBlossomValue)
        {
            BlossomingValue = RequiredBlossomValue;
        }
    }
    public void ResetBlossom()
    {
        BlossomingValue = 0;
        timer = 0f;
    }
}