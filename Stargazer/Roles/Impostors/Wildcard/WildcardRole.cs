using System.Collections.Generic;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Wildcard;

public class WildcardRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Wildcard";

    public string RoleDescription => "It's UNO time.";

    public string RoleLongDescription => "Use cards to apply negative effects to players during meetings.";

    public List<WildcardDeck.Cards> cards = new();

    public Color RoleColor => Palette.ImpostorRoleRed;

    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.WildcardRoleIcon,
        IntroSound = Assets.WildcardIntro
    };

    [RegisterEvent]
    public static void OnMurder(AfterMurderEvent e)
    {
        if (e.Source.AmOwner && e.Source.Data.Role is WildcardRole w)
        {
            w.cards.Add((WildcardDeck.Cards)UnityRandom.RandomRangeInt(0, 4));
        }
    }

    private WildcardDeck _deck;
    public override void OnMeetingStart()
    {
        if (PlayerControl.LocalPlayer.AmOwner && PlayerControl.LocalPlayer.Data.Role is not WildcardRole w) return;
        
        var toggleButton = Object.Instantiate(MeetingHud.Instance.MeetingAbilityButton, MeetingHud.Instance.transform);
        toggleButton.gameObject.SetActive(true);
        toggleButton.buttonLabelText.text = "Cards";
        toggleButton.SetInfiniteUses();
        var pos = toggleButton.gameObject.AddComponent<AspectPosition>();
        pos.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        pos.DistanceFromEdge = Vector3.one;
        pos.AdjustPosition();
        toggleButton.graphic.sprite = Assets.CardsButton.LoadAsset();
        toggleButton.GetComponent<PassiveButton>().OnClick.AddListener(new System.Action(() =>
        {
            if (MeetingHud.Instance.state == MeetingHud.VoteStates.Animating || MeetingHud.Instance.state == MeetingHud.VoteStates.Results) return;
            if (_deck) _deck.gameObject.Destroy();
            else
            {
                _deck = Object.Instantiate(Assets.WildcardDeckPrefab.LoadAsset()).GetComponent<WildcardDeck>();
            }
        }));
    }

    public override void OnVotingComplete()
    {
        if (_deck) _deck.gameObject.Destroy();
    }
}