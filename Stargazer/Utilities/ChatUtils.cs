using System;
using UnityEngine;

namespace Stargazer.Utilities;

public static class ChatUtils
{
    public static ChatBubble CreateAndShowChatWarning(this ChatController chat, string warningText)
    {
        ChatBubble pooledBubble = chat.GetPooledBubble();
        try
        {
            pooledBubble.transform.SetParent(chat.scroller.Inner);
            pooledBubble.transform.localScale = Vector3.one;
            pooledBubble.SetRight();
            pooledBubble.SetWarning(warningText);
            pooledBubble.AlignChildren();
            chat.AlignAllBubbles();
            if (!chat.IsOpenOrOpening && chat.notificationRoutine == null)
                chat.notificationRoutine = chat.StartCoroutine(chat.BounceDot());
            SoundManager.Instance.PlaySound(chat.warningSound, false);
        }
        catch (Exception ex)
        {
            ChatController.Logger.Error(ex.ToString());
            chat.chatBubblePool.Reclaim(pooledBubble);
        }

        return pooledBubble;
    }
}