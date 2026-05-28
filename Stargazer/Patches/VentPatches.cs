using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using Reactor.Utilities;
using Stargazer.Roles.Impostors.Carrier;
using Stargazer.Utilities;
using UnityEngine;

namespace Stargazer.Patches;

public class VentPatches
{
    [RegisterEvent]
    public static void Vent_Use_Postfix(PlayerUseEvent e)
    {
        if (!e.IsVent) return;

        var vent = e.Usable.TryCast<Vent>();
        foreach (var b in vent.gameObject.GetComponentsInChildren<DeadBody>())
        {
            b.bodyRenderers[0].enabled = true;
            b.transform.SetParent(null);
            Coroutines.Start(RFSEffects.CoMoveArc(b.transform, vent.transform.position, Vector2.Lerp(vent.transform.position, PlayerControl.LocalPlayer.GetTruePosition(), 0.8f), 0.4f));
            b.transform.localScale = new(0.7f, 0.7f, 1);
            Coroutines.Start(RFSEffects.Boop(b.transform, 0.4f, 1.6f, 0.4f));
        }
    }
}