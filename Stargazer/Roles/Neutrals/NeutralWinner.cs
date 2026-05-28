using MiraAPI.Modifiers.Types;
using Stargazer.Utilities;

namespace Stargazer.Roles.Neutrals;

public class NeutralWinner : GameModifier
{
    public override string ModifierName => "Neutral Winner";
    public override bool HideOnUi => false;

    public override int GetAmountPerGame()
    {
        return 0;
    }

    public override int GetAssignmentChance()
    {
        return 0;
    }

    public override string GetDescription()
    {
        return "You have won! sit back watch the rest of the game unfold!";
    }
}