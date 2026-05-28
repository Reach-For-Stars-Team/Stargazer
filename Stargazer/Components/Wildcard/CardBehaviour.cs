using System;
using BepInEx.Logging;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
    public Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;
    private Vector2 lastMousePosition;
    public System.Action<int> OnDropAccepted;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup   = GetComponent<CanvasGroup>();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = UnityEngine.Object.FindObjectOfType<Canvas>();

        // Wire up drag events through EventTrigger instead of interfaces
        var trigger = gameObject.AddComponent<EventTrigger>();

        var beginDrag = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        beginDrag.callback.AddListener((System.Action<BaseEventData>)((e) => OnBeginDrag(e.Cast<PointerEventData>())));
        trigger.triggers.Add(beginDrag);

        var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        drag.callback.AddListener((System.Action<BaseEventData>)((e) => OnDrag(e.Cast<PointerEventData>())));
        trigger.triggers.Add(drag);

        var endDrag = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
        endDrag.callback.AddListener((System.Action<BaseEventData>)((e) => OnEndDrag(e.Cast<PointerEventData>())));
        trigger.triggers.Add(endDrag);
    }

    private void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent   = transform.parent;
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
        lastMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }

    private void OnDrag(PointerEventData eventData)
    {
        Vector2 current = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        rectTransform.anchoredPosition += (current - lastMousePosition) / canvas.scaleFactor;
        lastMousePosition = current;
        PlayerControl.LocalPlayer.StartCoroutine(Effects.Rotate2D(transform, 0, Mathf.Clamp(UnityRandom.RandomRangeInt(-40, 40), -10, 10), 0));
    }

    private void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        ReturnToOrigin();
    }

    public void ReturnToOrigin()
    {
        if (originalParent == null) return;
        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = originalPosition;
    }

    public void SetAppearance(WildcardDeck.Cards card)
    {
        GetComponent<Image>().sprite = Stargazer.Assets.Bundle.LoadAsset<Sprite>(card.ToString());
    }
}