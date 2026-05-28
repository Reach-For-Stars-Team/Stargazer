using System.Collections;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Actor;

public class ActModifier(PlayerControl Source) : GameModifier
{
    public override string ModifierName => "Acted On";
    public override int GetAssignmentChance() => 0;
    public override bool Unique => false;
    public int Percentage { get; set; }
    public override int GetAmountPerGame() => 0;
    private TextMeshPro _NamePercentage;
    public void OnAct(int Gain)
    {
        Percentage += Gain;
        HasBeenActedOn = true;
        if (PlayerControl.LocalPlayer == Source)
        {
            if (_NamePercentage == null)
            {
                _NamePercentage = UnityObject.Instantiate(Player.cosmetics.nameText,
                    Player.cosmetics.nameTextContainer.transform);
                _NamePercentage.color = Color.yellow;
                _NamePercentage.transform.localPosition = new(0, 0.25f, 0);
                _NamePercentage.fontSize = 1.75f;
            }

            _NamePercentage.text = $"{Percentage}%";
            Coroutines.Start(CoAnimatePercentage(Gain));
        }
        
        if (Percentage >= 100) Reveal();
    }

    public void Reveal()
    {
            if (Player.Data.Role.IsImpostor) 
            {
                Player.cosmetics.SetNameColor(Palette.ImpostorRed);
                _NamePercentage.text = "Impostor";
                _NamePercentage.color = Palette.ImpostorRed;
                Player.MyPhysics.SetBodyType(PlayerBodyTypes.Seeker);
                SoundManager.Instance.PlaySound(Player.MyPhysics.ImpostorDiscoveredSound, false);
            }
            else if (!Player.Data.Role.IsImpostor) 
            {
                Player.cosmetics.SetNameColor(Palette.CrewmateBlue);
                _NamePercentage.text = "Crewmate";
                _NamePercentage.color = Palette.CrewmateBlue;
            }
    }

    public IEnumerator CoAnimatePercentage(int Gain)
    {
        var t = new GameObject("PercentageLabel").AddComponent<TextMeshPro>();
        t.transform.position = new(Player.transform.position.x, Player.transform.position.y, -100);
        t.font = HudManager.Instance.TaskPanel.taskText.font;
        t.fontSize = 2f;
        t.alignment = TextAlignmentOptions.Center;
        t.material = HudManager.Instance.TaskPanel.taskText.material;
        t.SetOutlineColor(Color.black);
        t.SetOutlineThickness(0.2f);
        t.text = $"+{Gain.ToString()}%";
        if (Percentage >= 100) t.text = $"Revealed!";
        
        Player.StartCoroutine(Effects.ScaleIn(t.transform, 2, 1, 0.3f));
        Player.StartCoroutine(Effects.ColorFade(t, Color.white, Color.yellow, 0.5f));
        yield return Effects.Bounce(t.transform, 0.3f, 0.3f);
        yield return new WaitForSeconds(0.7f);
        t.gameObject.Destroy();
        yield break;
    }

    public bool HasBeenActedOn { get; set; } = false; //if the player has been interacted with this round
    public override void OnMeetingStart()
    {
        if (!Player.AmOwner) return;
        if (!HasBeenActedOn) return;
        
        var notification = Helpers.CreateAndShowNotification(
            $"Someone has acted on you! ({Percentage.ToString()}%)", Color.yellow);
        notification.transform.localPosition = new Vector3(0f, 1f, -20f);
        
        HasBeenActedOn = false;
    }
}