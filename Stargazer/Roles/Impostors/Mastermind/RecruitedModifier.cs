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


namespace Stargazer.Roles.Impostors.Mastermind;

public class RecruitedModifier() : GameModifier
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
        }

    }

    public override bool? CanVent()
    {
        return OptionGroupSingleton<MastermindOptions>.Instance.RecruitedCanVent.Value;
    }
//artifacts from original idea

    public override bool? DidWin(GameOverReason reason)
    {
        return reason is GameOverReason.ImpostorsByKill || reason is GameOverReason.ImpostorsBySabotage || 
               reason is GameOverReason.ImpostorsByVote || reason is GameOverReason.CrewmateDisconnect || 
               reason is GameOverReason.HideAndSeek_ImpostorsByKills;
    }
    

    



    
}