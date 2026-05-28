using System;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Map;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using Stargazer.Utilities;
using UnityEngine;

namespace Stargazer.Modifiers.Game.Universal.Flash;

public class FlashModifier : GameModifier
{
    public override string ModifierName => "Flash";

    public override string GetDescription()
    {
        return "Go off with a bang! Flash-bang your killer on death!";
    }

    public override int GetAssignmentChance()
    {
        return OptionGroupSingleton<FlashModifierOptions>.Instance.AssignmentChance;
    }

    public override int GetAmountPerGame()
    {
        return OptionGroupSingleton<FlashModifierOptions>.Instance.AmountPerGame;
    }

    [RegisterEvent]
    public static void OnMurder(AfterMurderEvent e)
    {
        if (e.Source.AmOwner && e.Target.HasModifier<FlashModifier>())
        {
            e.Source.AddModifier<FlashbangedModifier>();
        }
    }
}