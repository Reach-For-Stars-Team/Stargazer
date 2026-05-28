using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Components;
using Stargazer.Networking;
using Stargazer.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Analyst;

public class AnalyzeButton : CustomActionButton<DeadBody>
{
    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.Data.Role is AnalystRole r && Target)
        {
            Target.TryGetComponent(out AnalystDeadBodyCache cache);
            if (cache == null) return;
            r.Data = cache.data;
            
            //Disable reporting
            cache.Destroy();
            Target.GetComponent<PassiveButton>().Destroy(); 
            Target.enabled = false;
        }

        Helpers.CreateAndShowNotification(
            $"You analyzed {PlayerControlUtils.GetPlayerById(Target.ParentId).Data.PlayerName}'s body, you'll get more information during the next meeting!",
            RFSPalette.AnalystRoleColor);
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is AnalystRole;
    }

    public override string Name => "Analyze";

    public override float Cooldown => 30;

    public override LoadableAsset<Sprite> Sprite => Assets.AnalyzeButton;

    public override DeadBody GetTarget()
    {
        return PlayerControl.LocalPlayer.GetNearestDeadBody(Distance);
    }

    public override void SetOutline(bool active)
    {
        Target?.bodyRenderers[0].SetOutline(active ? RFSPalette.AnalystRoleColor : Color.clear);
    }
}