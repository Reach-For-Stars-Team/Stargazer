using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;

namespace Stargazer.Modifiers;

public class InvisibleModifier :  BaseModifier
{
    public override string ModifierName => "Invis";
    public override bool HideOnUi => false;
    public override bool ShowInFreeplay => false;

    public override void OnDeactivate()
    {
        Player.Visible = true;
    }
}