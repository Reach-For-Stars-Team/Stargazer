using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stargazer.Components;

public class PlayerDetectionBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
    public Action<PlayerControl> OnEnter;
    public Action<PlayerControl> OnStay;
    public Action<PlayerControl> OnExit;

    public bool canTrigger = true;

    public Vector2 LocalOffset = Vector2.zero;
    public Vector2 Radius = new Vector2(0.25f, 0.25f);

    private readonly List<byte> playersInside = new();

    public void FixedUpdate()
    {
        if (!canTrigger || PlayerControl.AllPlayerControls == null)
        {
            return;
        }

        bool advancedMode =
            OnStay != null ||
            LocalOffset != Vector2.zero ||
            !Mathf.Approximately(Radius.x, 0.25f) ||
            !Mathf.Approximately(Radius.y, 0.25f);

        Vector2 center = transform.TransformPoint(LocalOffset);

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.IsDead)
            {
                continue;
            }

            if (!advancedMode && !player.AmOwner)
            {
                continue;
            }

            Vector2 diff = (Vector2)player.transform.position - center;

            float radiusX = Mathf.Max(0.01f, Radius.x);
            float radiusY = Mathf.Max(0.01f, Radius.y);

            bool isInside =
                (diff.x * diff.x) / (radiusX * radiusX) +
                (diff.y * diff.y) / (radiusY * radiusY) <= 1f;

            bool wasInside = playersInside.Contains(player.PlayerId);

            if (isInside && !wasInside)
            {
                playersInside.Add(player.PlayerId);
                OnEnter?.Invoke(player);
            }

            if (isInside)
            {
                OnStay?.Invoke(player);
            }

            if (!isInside && wasInside)
            {
                playersInside.Remove(player.PlayerId);
                OnExit?.Invoke(player);
            }
        }
    }
}
