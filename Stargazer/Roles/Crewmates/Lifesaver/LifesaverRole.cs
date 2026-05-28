using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Stargazer.Components;
using UnityEngine;
using UnityEngine.Serialization;

namespace Stargazer.Roles.Crewmates.Lifesaver;

public class LifesaverRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Lifesaver";

    public string RoleDescription => "Revive dead players!";

    public string RoleLongDescription => "Finish your tasks to revive a dead player.";

    public Color RoleColor => RFSPalette.LifesaverRoleColor;

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.ReviveButton
    };

    public bool hasFirstAidKit = false;

    public void SetFirstAidKit(bool b)
    {
        hasFirstAidKit = b;
        HudManager.Instance.SetHudActive(true);
    }

    [RegisterEvent]
    public static void OnTaskCompleted(CompleteTaskEvent e)
    {
        if (e.Player.Data.Role is LifesaverRole l)
        {
            if (e.Player.GetTasksLeft() == 0)
            {
                l.hasFirstAidKit = true;
            }
        }
    }
}