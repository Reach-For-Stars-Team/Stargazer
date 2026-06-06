using System;
using System.Collections;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Stargazer.Components;
using Stargazer.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Events;
using MiraAPI.GameOptions.OptionTypes;
using Rewired;
using AmongUs.GameOptions;  
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using MiraAPI.Roles;
using MiraAPI.Patches;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using Stargazer.Roles.Impostors.Impostors;
using TMPro;


namespace Stargazer.Roles.Impostors.Mastermind;

public class RecruitedModifier : GameModifier
{
    public override string ModifierName => "Recruited By Mastermind";
    public override bool HideOnUi => false;
    public override bool ShowInFreeplay => true;
    public override int GetAmountPerGame()
    {
        return 0;
    }

    public override int Priority()
    {
        return 10;
    }

    public override int GetAssignmentChance()
    {
        return 0;
    }

    public override void OnActivate()
    {
        if (Player.AmOwner)
        {
            CustomButtonSingleton<RecruitedKill>.Instance.Button?.Show();
            Coroutines.Start(CoAnimate());
        }
    }

    private IEnumerator CoAnimate()
    {
        var label = Helpers.CreateTextLabel("TransitionText", HudManager.Instance.transform,
            AspectPosition.EdgeAlignments.Top, new Vector3(0, 3f), textAlignment:TextAlignmentOptions.Top);
        string message = "Your WILL is CHANGING...";
        string message2 = "You've joined the bloody side.";
        foreach (var c in message)
        {
            label.text += c;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
        }

        Player.StartCoroutine(Effects.ColorFade(label, Color.white, Palette.ImpostorRed, 2));
        yield return new WaitForSeconds(0.5f);
        label.text += "\n\n\n\n\n\n";
        foreach (var c in message2)
        {
            label.text += c;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
        }
        HudManager.Instance.FadeScreen(Color.red, Color.red.ToClearColor(), 0.7f);
        Player.MyPhysics.SetBodyType(PlayerBodyTypes.Seeker);
        yield return new WaitForSeconds(0.8f);
        Coroutines.Start(RFSEffects.ColorFadeAndDestroy(label, Palette.ImpostorRed, Palette.ImpostorRed.ToClearColor(), 0.2f));
    }

    public override bool? CanVent()
    {
        return OptionGroupSingleton<MastermindOptions>.Instance.RecruitedCanVent.Value;
    }
//artifacts from original idea

    public override bool? DidWin(GameOverReason reason)
    {
        return reason is GameOverReason.ImpostorsByKill or 
                   GameOverReason.ImpostorsBySabotage or 
                   GameOverReason.ImpostorsByVote or 
                   GameOverReason.CrewmateDisconnect or 
                   GameOverReason.HideAndSeek_ImpostorsByKills;
    }
}