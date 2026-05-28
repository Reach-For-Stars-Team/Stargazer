using MiraAPI.Modifiers;

namespace Stargazer.Roles.Impostors.Florist;

public class SlowedDownModifier : BaseModifier
{
    public override string ModifierName => "Slowed Down";

    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        Player.MyPhysics.Speed /= 2;
    }

    public override void OnDeactivate()
    {
        Player.MyPhysics.Speed *= 2; //TODO Configurable?
    }
}