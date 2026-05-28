using System.Linq;
using Il2CppSystem;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Networking;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Taskmaster;

public class TaskAbility : CustomActionButton<PlayerControl>
{
    public override string Name { get; } = "Do Task";
    
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    
    public override float Cooldown => OptionGroupSingleton<TaskmasterOptions>.Instance.AbilityCooldown.Value;

    public override LoadableAsset<Sprite> Sprite { get; } = Assets.DoTaskButton;

    protected override void OnClick()
    {
        Target?.RpcCompleteTask(Target.myTasks.ToArray().Where(x => !x.IsComplete).Random().Id);
        
    }

    public override PlayerControl GetTarget()
    {
        TaskmasterOptions.AbilityTargets opt =
            (TaskmasterOptions.AbilityTargets)OptionGroupSingleton<TaskmasterOptions>.Instance.ValidTargets.Value;
        return PlayerControl.LocalPlayer.GetClosestPlayer(false,
            Distance,
            true, 
            true, x => (x.Data.IsDead && opt is TaskmasterOptions.AbilityTargets.DeadOnly) || (!x.Data.IsDead  && opt is TaskmasterOptions.AbilityTargets.AliveOnly) || opt == TaskmasterOptions.AbilityTargets.Both);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new(RFSPalette.TaskmasterRoleColor));
    }
    public override bool Enabled(RoleBehaviour role)
    {
        return role is TaskmasterRole;
    }

    public override Color TextOutlineColor => RFSPalette.TaskmasterRoleColor;

    public override bool IsTargetValid(PlayerControl target)
    {
        return target != null && target.myTasks.ToArray().Count(x => !x.IsComplete) > 0;
    }
}