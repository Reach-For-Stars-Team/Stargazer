using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using Stargazer.Networking;
using Stargazer.Roles.Impostors.Wildcard;
using Stargazer.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WildcardDeck(IntPtr ptr) : MonoBehaviour(ptr)
{
    public Il2CppReferenceField<CardBehaviour> cardPrefab;
    public Il2CppReferenceField<CardReleaseZone> cardReleaseZonePrefab;
    public List<CardBehaviour> cards = new();
    public List<CardReleaseZone> cardReleases = new();
    public Il2CppReferenceField<Transform> cardsParent;
    public Il2CppReferenceField<AudioClip> cardSound;
    public enum Cards
    {
        Block = 0,
        PlusTwo = 1,
        PlusFour = 2,
        Cosmetic = 3,
        Mute = 4
    }
    private void Start()
    {
        if (PlayerControl.LocalPlayer.Data.Role is not WildcardRole w) return;

        var canvas = GetComponent<Canvas>();
        foreach (var votearea in MeetingHud.Instance.playerStates)
        {
            var release = Instantiate(cardReleaseZonePrefab.Value, transform);
            release.playerId = votearea.TargetPlayerId;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(votearea.transform.position);

            // Convert screen position → local position inside the canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                GetComponent<RectTransform>(),
                screenPos,
                null,
                out Vector2 localPos
            );

            var rectTransform = release.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = localPos;
            rectTransform.sizeDelta = Screen.height / (Camera.main.orthographicSize * 2)*votearea.Background.size;
        }

        float i = 0;
        foreach (var c in w.cards)
        {
            var newCard = Instantiate(cardPrefab.Value, cardsParent.Value);
            newCard.OnDropAccepted = new Action<int>((int i) => OnSelectCard(newCard, c, (byte)i));
            newCard.SetAppearance(c);
            cards.Add(newCard);
            MeetingHud.Instance.StartCoroutine(Effects.ActionAfterDelay(i / 5, new System.Action((() => Coroutines.Start(RFSEffects.Boop(newCard.transform, 0, 2, 0.5f))))));
            i++;
        }
    }

    public void OnSelectCard(CardBehaviour button, Cards card, byte playerId)
    {
        if (PlayerControl.LocalPlayer.Data.Role is not WildcardRole w) return;
        AudioSource.PlayClipAtPoint(cardSound, Camera.main.transform.position, 1);
        PlayerControl.LocalPlayer.RpcUseWildCard(playerId, (uint)card);
        w.cards.Remove(card);
        button.gameObject.Destroy();
        if (w.cards.Count == 0) gameObject.Destroy();
    }
}