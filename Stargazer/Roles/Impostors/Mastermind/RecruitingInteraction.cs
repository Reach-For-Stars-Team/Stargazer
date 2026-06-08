using System.Collections.Generic;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;

namespace Stargazer.Roles.Impostors.Mastermind;

public class RecruitingInteraction
{
    public static List<RecruitingInteraction> Interactions = new();
    
    public uint id;
    public PlayerControl mastermind;
    public ChoiceTypes mastermindChoice;
    public ChoiceTypes toBeRecruitedChoice;
    public PlayerControl toBeRecruited;
    public RecruitingInteractionState state = RecruitingInteractionState.Choosing;

    public RecruitingInteraction(PlayerControl mastermind, PlayerControl toBeRecruited)
    {
        this.mastermind = mastermind;
        this.toBeRecruited = toBeRecruited;
        id = mastermind.PlayerId;
        
        Interactions.Add(this);
    }

    public void Update(PlayerControl source, ChoiceTypes choice)
    {
        if (source == mastermind) mastermindChoice = choice;
        else if (source == toBeRecruited) toBeRecruitedChoice = choice;
    
        if (mastermindChoice is ChoiceTypes.None || toBeRecruitedChoice is ChoiceTypes.None)
        {
            var minigame = Minigame.Instance.TryCast<RecruiterMinigame>();
            if (Minigame.Instance && minigame && minigame.interaction.id == id)
            {
                if (source == mastermind)
                    minigame.labelTextRecruiter.Value.text = $"{(mastermind.AmOwner ? "YOU" : "Mastermind")} <b>(Chosen)</b>";
                else if (source == toBeRecruited)
                    minigame.labelTextToBeRecruited.Value.text = $"{(toBeRecruited.AmOwner ? "YOU" : "Target")} <b>(Chosen)</b>";
            }
            return;
        }
    
        SetWinner();
    }

    private void SetWinner()
    {
        if (mastermindChoice == toBeRecruitedChoice)
        {
            state = RecruitingInteractionState.Tie;
            return;
        }

        if (mastermindChoice == ChoiceTypes.Rock)
        {
            if (toBeRecruitedChoice == ChoiceTypes.Scissors)
                state = RecruitingInteractionState.Success;
            else if (toBeRecruitedChoice == ChoiceTypes.Paper)
                state = RecruitingInteractionState.Failure;
        }
        if (mastermindChoice == ChoiceTypes.Scissors)
        {
            if (toBeRecruitedChoice == ChoiceTypes.Paper)
                state = RecruitingInteractionState.Success;
            else if (toBeRecruitedChoice == ChoiceTypes.Rock)
                state = RecruitingInteractionState.Failure;
        }
        if (mastermindChoice == ChoiceTypes.Paper)
        {
            if (toBeRecruitedChoice == ChoiceTypes.Rock)
                state = RecruitingInteractionState.Success;
            else if (toBeRecruitedChoice == ChoiceTypes.Scissors)
                state = RecruitingInteractionState.Failure;
        }
        var minigame = Minigame.Instance.TryCast<RecruiterMinigame>();
        if (Minigame.Instance && minigame && minigame.interaction.id == id)
        {
            if (state is RecruitingInteractionState.Success) Coroutines.Start(minigame.Animate("<b>Recruited!</b>"));
            else if (state is RecruitingInteractionState.Tie) Coroutines.Start(minigame.Animate("<b>Tie!</b>"));
            else
            {
                minigame.imageRecruiter.Value.sprite = minigame.recruiterRevealedImage.Value;
                PlayerMaterial.SetColors(minigame.interaction.mastermind.cosmetics.ColorId, minigame.imageRecruiter.Value.material);
                Coroutines.Start(minigame.Animate("<b>Identity Revealed!</b>"));
            }
            
            minigame.labelTextRecruiter.Value.text = $"Mastermind <b>({mastermindChoice})</b>";
            minigame.labelTextToBeRecruited.Value.text = $"YOU <b>({toBeRecruitedChoice})</b>";

            mastermind.StartCoroutine(Effects.ActionAfterDelay(2, new System.Action(() => minigame.gameObject.Destroy())));
            if (state ==  RecruitingInteractionState.Success) toBeRecruited.RpcAddModifier<RecruitedModifier>();
            else if (state == RecruitingInteractionState.Failure && toBeRecruited.AmOwner)
            {
                mastermind.MyPhysics.SetBodyType(PlayerBodyTypes.Seeker);
                SoundManager.Instance.PlaySound(mastermind.MyPhysics.ImpostorDiscoveredSound, false);
            }
        }

        Interactions.Remove(this);
    }
}

public enum ChoiceTypes
{
    None,
    Rock,
    Paper,
    Scissors,
}

public enum RecruitingInteractionState
{
    Choosing,
    Success,
    Failure,
    Tie
}