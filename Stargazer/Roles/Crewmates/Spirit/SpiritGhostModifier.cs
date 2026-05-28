using System.Collections;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using Stargazer.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Spirit;

public class SpiritGhostModifier : BaseModifier
{
    public override string ModifierName => "Spirit Ghost";
    private GameObject fakePlayer;
    public override void OnActivate()
    {
        fakePlayer = PlayerControlUtils.CreateFakePlayer(Player);
        Player.Die(DeathReason.Exile, false);
    }
    
    public override void OnDeactivate()
    {
        Player.NetTransform.SnapTo(fakePlayer.transform.position);
        fakePlayer.gameObject.Destroy();
        Player.cosmetics.TogglePet(true);
        Player.Revive();
    }

    public override void FixedUpdate()
    {
        if (Player.AmOwner == false) return;
        HudManager.Instance.Chat.chatButton.gameObject.SetActive(false);
    }

    [RegisterEvent]
    public static void BeforeGameEndEvent(BeforeGameEndEvent e)
    {
        var alivePlayers = Helpers.GetAlivePlayers();
        int impsCount = alivePlayers.Count(x => x.Data.Role.IsImpostor);
        int crewCount = alivePlayers.Count - impsCount;
        int spiritGhostsCount = ModifierUtils.GetPlayersWithModifier<SpiritGhostModifier>().Count();
        if (crewCount + spiritGhostsCount > impsCount && impsCount > 0 && e.Reason == GameOverReason.ImpostorsByKill) e.Cancel();
    }
}