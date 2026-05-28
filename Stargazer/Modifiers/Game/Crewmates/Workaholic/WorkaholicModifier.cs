using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Map;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using Stargazer.Utilities;
using UnityEngine;

namespace Stargazer.Modifiers.Game.Crewmates.Workaholic;

public class WorkaholicModifier : GameModifier
{
    public override string ModifierName => "Workaholic";

    public override string GetDescription()
    {
        return $"You must finish a set amount of tasks to survive\n" +
               $"{TasksDoneThisRound}/{OptionGroupSingleton<WorkaholicModifierOptions>.Instance.TasksQuota.Value} Tasks completed this round.";
    }

    public override int GetAssignmentChance()
    {
        return OptionGroupSingleton<WorkaholicModifierOptions>.Instance.AssignmentChance;
    }

    public override int GetAmountPerGame()
    {
        return OptionGroupSingleton<WorkaholicModifierOptions>.Instance.AmountPerGame;
    }

    private int TasksDoneThisRound = 0;

    public override void OnMeetingStart()
    {
        if (Player.Data.IsDead) return;
        
        if (TasksDoneThisRound <= OptionGroupSingleton<WorkaholicModifierOptions>.Instance.TasksQuota)
        {
            Player.Die(DeathReason.Kill, true);
            var popup = HudManager.Instance.SpawnHnSPopUp(Player, "Died from not doing enough tasks!");
            var pos = popup.transform.position;
            pos.z -= 10;
            popup.transform.position = pos;

            if (Player.AmOwner)
            {
                HudManager.Instance.SetHudActive(false);
            }
        }
        
        TasksDoneThisRound = 0;
    }

    [RegisterEvent]
    public static void OnTaskCompleted(CompleteTaskEvent e)
    {
        e.Player.TryGetComponent(out WorkaholicModifier m);
        if (m == null) return;
        m.TasksDoneThisRound++;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.TeamType == RoleTeamTypes.Crewmate;
    }
}