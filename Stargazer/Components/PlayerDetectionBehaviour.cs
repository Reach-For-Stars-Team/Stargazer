using System;
using UnityEngine;

namespace Stargazer.Components;

public class PlayerDetectionBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
    public Action<PlayerControl> OnEnter;
    public Action<PlayerControl> OnExit;
    public bool canTrigger = true;

    public void FixedUpdate()
    {
        if (canTrigger && Vector2.Distance(transform.position, PlayerControl.LocalPlayer.transform.position) < 0.25f)
        {
            OnEnter?.Invoke(PlayerControl.LocalPlayer);
        }
    }
}