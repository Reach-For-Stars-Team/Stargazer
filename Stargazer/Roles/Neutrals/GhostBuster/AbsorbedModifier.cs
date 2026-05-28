using MiraAPI.Modifiers;
using Stargazer.Utilities;

namespace Stargazer.Roles.Neutrals.GhostBuster;

public class AbsorbedModifier(PlayerControl source) : BaseModifier
{
    public override string ModifierName => "Absorbed";
    public override void OnActivate()
    {
        if (!Player.AmOwner) return;
        
        HudManager.Instance.PlayerCam.SetTarget(source);
        HudManager.Instance.SetHudActive(false);
        HudManager.Instance.SpawnTextOverlay("You've been absorbed!");
    }

    public override void FixedUpdate()
    {
        Player.Visible = false;
        Player.transform.position = source.transform.position;
    }

    public override void OnMeetingStart()
    {
        Player.RemoveModifier<AbsorbedModifier>();
    }
}