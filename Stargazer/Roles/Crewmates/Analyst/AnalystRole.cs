using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Stargazer.Components;
using Stargazer.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Stargazer.Roles.Crewmates.Analyst;

public class AnalystRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Analyst";

    public string RoleDescription => "Look for clues!";

    public string RoleLongDescription => "Analyze dead bodies to gather crucial information.";

    public Color RoleColor => RFSPalette.AnalystRoleColor;

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.AnalystRoleIcon
    };

    public AnalystDeadBodyData Data { get; set; }

    public override void OnMeetingStart()
    {
        if (Player.AmOwner && Data != null)
        {
            var bubble = HudManager.Instance.Chat.CreateAndShowChatWarning(Data.GetInfoMessage());
            bubble.Background.color = Color.black;
        }

        Data = null!;
    }

    [RegisterEvent]
    public static void OnMurder(AfterMurderEvent e)
    {
        if (e.DeadBody)
        {
            e.DeadBody.gameObject.AddComponent<AnalystDeadBodyCache>().Initialize(e.Target, e.Source);
        }
    }
}

public class AnalystDeadBodyData(string playerName, RoleTypes killerRole)
{
    public string PlayerName = playerName;
    public RoleTypes KillerRole = killerRole;
    public float Lifetime = 0;

    public string GetInfoMessage()
    {
        return "<color=white>"+ "<size=9>Analyst Role Info</size>\n" + 
               "<b>" + 
               $"You analyzed {PlayerName}'s body!\n" +
               "\n" +
               $"● They were killed {(int)Lifetime} seconds ago.\n" +
               $"● Their killer's role was: {TranslationController.Instance.GetString(RoleManager.Instance.AllRoles.ToArray().First(x => x.Role == KillerRole).StringName)}\n";
    }
}