using System.Linq;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Stargazer.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Pyromaniac;

public class BurningModifier : TimedModifier
{
    public override string ModifierName => "Burning";
    
    public override float Duration => 5;

    public override bool ShowInFreeplay => true;

    public PlayerControl PyromaniacPlayer;
    
    private GameObject flame;
    
    public BurningModifier(PlayerControl source)
    {
        PyromaniacPlayer = source;
    }

    public override void OnActivate()
    {
        flame = UnityObject.Instantiate(Assets.flamePrefab.LoadAsset());
        flame.transform.parent = Player.transform;
        flame.transform.localPosition = new(0, 0, -5);
        flame.layer = LayerMask.NameToLayer("Players");
        flame.transform.localScale = new(2, 2, 1);
        Coroutines.Start(RFSEffects.Boop(flame.transform, 4f, 1, 0.5f));
        if (!Player.HasModifier<DousedModifier>()) return;
        Player.RemoveModifier<DousedModifier>();
        Player.cosmetics.nameText.color = RFSPalette.BurningPlayerNameColor;
        if (Player.AmOwner)
        {
            var overlay = UnityObject.Instantiate(HudManager.Instance.FullScreen, HudManager.Instance.transform);
            overlay.transform.localPosition = new Vector3(0, 0, 10);
            overlay.gameObject.SetActive(true);
            Coroutines.Start(RFSEffects.ColorPulseAndDestroy(overlay,
                new Color32(255,
                    100,
                    0,
                    0),
                new Color32(255,
                    200,
                    0,
                    150),
                new Color32(255,
                    200,
                    0,
                    0),
                0.4f,
                4.6f));
        }
    }

    public override void OnTimerComplete()
    {
        base.OnTimerComplete();
        flame.Destroy();
        if (PyromaniacPlayer && PyromaniacPlayer.AmOwner) PyromaniacPlayer.RpcCustomMurder(Player, showKillAnim:false, playKillSound:false, teleportMurderer:false);
        else if (PyromaniacPlayer == null && Player.AmOwner) Player.RpcCustomMurder(Player, showKillAnim:false, playKillSound:false, teleportMurderer:false);
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (Player.AmOwner)
        {
            foreach (var p in Helpers.GetClosestPlayers(Player, 1f, false).Where(x => x.HasModifier<DousedModifier>()))
            {
                p.RpcAddModifier<BurningModifier>(); //It should work??? Oh wait it doesn't in freeplay im dum
            }
        }
    }
}