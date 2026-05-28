using System;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardReleaseZone(IntPtr ptr) : MonoBehaviour(ptr)
{
    // UnityEvent → System.Action, same reason as CardBehaviour
    public System.Action<CardBehaviour> onItemDropped;

    public Il2CppReferenceField<UnityEngine.UI.Image> highlightImage;
    public Color normalColor    = new Color(1, 0.2f, 0.2f, 0.25f);
    public Color highlightColor = new Color(0, 1, 0, 0.3f);
    public int playerId;

    void Start()
    {
        if (highlightImage.Value != null)
        {
            highlightImage.Value.sprite = Stargazer.Assets.Square.LoadAsset();
            highlightImage.Value.color = normalColor;
        }

        // Wire up pointer events through EventTrigger, same as CardBehaviour
        var trigger = gameObject.AddComponent<EventTrigger>();

        var pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        pointerEnter.callback.AddListener((System.Action<BaseEventData>)((e) => OnPointerEnter(e.Cast<PointerEventData>())));
        trigger.triggers.Add(pointerEnter);

        var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((System.Action<BaseEventData>)((e) => OnPointerExit(e.Cast<PointerEventData>())));
        trigger.triggers.Add(pointerExit);

        var drop = new EventTrigger.Entry { eventID = EventTriggerType.Drop };
        drop.callback.AddListener((System.Action<BaseEventData>)((e) => OnDrop(e.Cast<PointerEventData>())));
        trigger.triggers.Add(drop);
    }

    private void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        if (highlightImage.Value != null)
            highlightImage.Value.color = highlightColor;
    }

    private void OnPointerExit(PointerEventData eventData)
    {
        if (highlightImage.Value != null)
            highlightImage.Value.color = normalColor;
    }

    private void OnDrop(PointerEventData eventData)
    {
        if (highlightImage.Value != null)
            highlightImage.Value.color = normalColor;

        if (eventData.pointerDrag == null) return;
        CardBehaviour draggable = eventData.pointerDrag.GetComponent<CardBehaviour>();
        if (draggable == null) return;

        draggable.OnDropAccepted?.Invoke(playerId);
        onItemDropped?.Invoke(draggable);

        Debug.Log($"[DropZone] '{draggable.name}' dropped on '{gameObject.name}'");
    }
}