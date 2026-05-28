using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Stargazer.Networking;
using Stargazer.Roles.Neutrals.GhostBuster;
using Stargazer.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Stargazer.Components;
[RegisterInIl2Cpp]
public class GhostTrap(IntPtr ptr) : MonoBehaviour(ptr) 
{
    public static List<GhostTrap> allGhostTraps = new List<GhostTrap>();
    List<PlayerControl> trappedPlayers = new List<PlayerControl>();
    public PlayerControl source;
    ContactFilter2D filter = new();

    private void Start()
    {
        allGhostTraps.Add(this);
        id = allGhostTraps.Count;
    }
    
    private void OnDestroy()
    {
        allGhostTraps.Remove(this);
    }

    public int id;

    public void FixedUpdate()
    {
        if (isAnimating) return;
        if (!PlayerControl.LocalPlayer.Data.IsDead) return;
        if (Vector2.Distance(transform.position, PlayerControl.LocalPlayer.transform.position) <= 3f) PlayerControl.LocalPlayer.RpcTriggerGhostTrap(id);
    }

    public bool isAnimating = false;

    public IEnumerator CoAnimateTrapped(PlayerControl target)
    {
        isAnimating = true;
        trappedPlayers.Add(target);
        var pivot = new GameObject("Pivot").AddComponent<SpriteRenderer>();
        pivot.transform.SetParent(transform);
        pivot.transform.localScale = Vector3.one;
        pivot.transform.localPosition = Vector3.zero;
        pivot.sprite = Assets.Wind.LoadAsset();
        GameObject point = new("Point");
        point.transform.SetParent(pivot.transform);
        point.transform.position = target.transform.position;
        target.StartCoroutine(Effects.Rotate2D(pivot.transform, 0, 360*5, 3f));
        target.StartCoroutine(Effects.ScaleIn(pivot.transform, 1, 0, 3f));
        Coroutines.Start(RFSEffects.ColorFadeAndDestroy(pivot, Color.white, new(1, 1, 1, 0), 3.5f));
        Vector2 ogScale = target.transform.localScale;
        target.StartCoroutine(Effects.ScaleIn(target.transform, ogScale.x, 0, 3f));
        for (float t = 0; t < 3f; t += Time.deltaTime)
        {
            target.transform.position = point.transform.position;
            yield return null;
        }
        target.AddModifier<AbsorbedModifier>(source);
        var gb = source.Data.Role.TryCast<GhostBusterRole>();
        if (gb) gb.AddAbsorbedPlayer(target);
        target.transform.localScale = ogScale;
        isAnimating = false;
        yield break;
    }
}