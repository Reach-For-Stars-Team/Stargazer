using System.Collections.Generic;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
using Stargazer.Components.Minigames;
using Stargazer.Networking;
using Stargazer.Utilities;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Sheriff;

public class SheriffRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Sheriff";

    public string RoleDescription => "Take it into your own hands.";

    public string RoleLongDescription => "Shoot suspicious players to kill them during meetings.";

    public Color RoleColor => Palette.ImpostorRoleRed;
    public int availableBullets = 999; //TODO

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public bool pulledOutGun = false;
    public int loadedBullets = 0;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.WildcardRoleIcon,
        IntroSound = Assets.WildcardIntro
    };

    [RegisterEvent]
    public static void OnMurder(AfterMurderEvent e)
    {
        if (MeetingHud.Instance)
        {
            var voteArea = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == e.Target.PlayerId);
            voteArea.SetDisabled();
            var rb = voteArea.gameObject.AddComponent<Rigidbody2D>();
            rb.AddForce(new(2, 7), ForceMode2D.Impulse);
            voteArea.StartCoroutine(Effects.Rotate2D(voteArea.transform, 0, 180 * 10, 15));
            rb.gravityScale = 1.5f;
        }
    }
    //Event for selecting and shooting a player
    [RegisterEvent]
    public static void BeforeVoteEvent(BeforeVoteEvent e)
    {
        if (e.Player.AmOwner && e.Player.Data.Role is SheriffRole s && s.pulledOutGun)
        {
            s.pulledOutGun = false;
            e.Cancel();
            e.VoteArea.ClearButtons();
            e.Player.RpcShootPlayer(PlayerControlUtils.GetPlayerById(e.VoteArea.TargetPlayerId), s.loadedBullets);
            s.availableBullets -= s.loadedBullets;
            s.loadedBullets = 0;
        }
    }
    //Event to stop selecting during minigames.
    [RegisterEvent]
    public static void SelectEvent(MeetingSelectEvent e)
    {
        if (Minigame.Instance && (Minigame.Instance.TryCast<ShotMinigame>() || Minigame.Instance.TryCast<SheriffShootMinigame>()))
        {
            e.AllowSelect = false;
        }
    }

    public override void OnMeetingStart()
    {
        if (PlayerControl.LocalPlayer.AmOwner && PlayerControl.LocalPlayer.Data.Role is not SheriffRole r) return;
        
        loadedBullets = 0;
        var toggleButton = Object.Instantiate(MeetingHud.Instance.MeetingAbilityButton, MeetingHud.Instance.transform);
        toggleButton.gameObject.SetActive(true);
        toggleButton.buttonLabelText.text = "Shoot";
        toggleButton.SetInfiniteUses();
        var pos = toggleButton.gameObject.AddComponent<AspectPosition>();
        pos.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        pos.DistanceFromEdge = Vector3.one;
        pos.AdjustPosition();
        toggleButton.graphic.sprite = Assets.CardsButton.LoadAsset();
        toggleButton.GetComponent<PassiveButton>().OnClick.AddListener(new System.Action(() =>
        {
            if (MeetingHud.Instance.state == MeetingHud.VoteStates.Animating || MeetingHud.Instance.state == MeetingHud.VoteStates.Results) return;
            if (Minigame.Instance) Minigame.Instance.gameObject.Destroy();
            else
            {
                SheriffShootMinigame.CreateAndOpen(PlayerControl.LocalPlayer.Data.Role.TryCast<SheriffRole>());
            }
        }));
    }

    public override void OnVotingComplete()
    {
        if (Minigame.Instance) Minigame.Instance.gameObject.Destroy();
    }
}