using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Lifesaver;

public class RevivedModifier : BaseModifier
{
    public override string ModifierName => "Revived";
    private ArrowBehaviour arrow;
    public override void OnActivate()
    {
        if (PlayerControl.LocalPlayer.Data.Role.TeamType is RoleTeamTypes.Impostor && !Player.AmOwner)
        {
            arrow = Helpers.CreateArrow(HudManager.Instance.transform, Color.green);
        }
    }

    public override void FixedUpdate()
    {
        if (arrow == null) return;

        arrow.target = Player.GetTruePosition();
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        if (arrow)
        {
            arrow.gameObject.Destroy();
        }
    }
}