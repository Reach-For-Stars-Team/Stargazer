using System.Collections;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Stargazer.Components;
using Stargazer.Components.Tasks;
using Stargazer.Roles.Crewmates.Actor;
using Stargazer.Roles.Crewmates.Lightener;
using Stargazer.Roles.Crewmates.Paranoiac;
using Stargazer.Roles.Impostors.Cowboy;
using Stargazer.Roles.Impostors.Sleepcaster;
using Stargazer.Roles.Neutrals.GhostBuster;
using Stargazer.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using Stargazer.Components.Minigames;
using Stargazer.Roles.Impostors.Florist;
using Stargazer.Roles.Neutrals.Roleless;
using UnityEngine;
using UnityEngine.ProBuilder;
using Helpers = MiraAPI.Utilities.Helpers;
using Object = UnityEngine.Object;

namespace Stargazer.Networking;

public static class RPCHandler
{
    [MethodRpc((uint)RPC.Yeehaw)]
    public static void RpcLasso(this PlayerControl source, PlayerControl Target)
    {
        var line = new GameObject("Lasso").AddComponent<LineRenderer>();
        line.textureMode = LineTextureMode.Tile;
        line.material.mainTexture = Assets.DigButton.LoadAsset().texture;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.loop = false;
        line.useWorldSpace = true;
        line.SetWidth(0.25f, 0.25f);
        Color lineColor = new(0.8f, 0.6f, 0, 1);
        Color lineColor2 = new(0.5f, 0.3f, 0, 1);
        line.SetColors(lineColor, lineColor2);
        Coroutines.Start(CoLasso(source, Target, line));
    }

    private static IEnumerator CoLasso(PlayerControl source, PlayerControl Target, LineRenderer line)
    {
        var totalDur = OptionGroupSingleton<CowboyOptions>.Instance.PullDuration.Value;
        source.moveable = false;
        source.NetTransform.Halt();
        Target.moveable = false;
        Target.NetTransform.Halt();

        float grabDuration = totalDur * 0.7f;

        line.material = Assets.ropeMaterial.LoadAsset();
        line.textureMode = LineTextureMode.Tile;

        int segments = 20;
        line.positionCount = segments;

        float waveAmplitude = 0.4f;
        float waveFrequency = 0.25f;
        float waveSpeed = 8f;

        for (float t = 0f; t < grabDuration; t += Time.deltaTime)
        {
            Vector3 endPos = Vector3.Lerp(
                source.transform.position,
                Target.transform.position,
                t / grabDuration
            );

            Vector3 startPos = source.transform.position;

            Vector3 direction = (endPos - startPos).normalized;

            Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward);

            for (int i = 0; i < segments; i++)
            {
                float normalized = i / (float)(segments - 1);

                Vector3 point = Vector3.Lerp(startPos, endPos, normalized);
                
                float wave =
                    Mathf.Sin(normalized * waveFrequency * Mathf.PI +
                              Time.time * waveSpeed)
                    * waveAmplitude
                    * (1f - normalized); 

                point += perpendicular * wave;

                line.SetPosition(i, point);
            }

            yield return null;
        }

        line.positionCount = 2;
        line.SetPosition(0, source.transform.position);
        line.SetPosition(1, Target.transform.position);

        Target.AddModifier<WrangledModifier>();

        yield return new WaitForSeconds(0.7f);

        float pullDuration = totalDur * 0.3f;

        for (float t = 0f; t < pullDuration; t += Time.deltaTime)
        {
            Vector3 newPosition = Vector3.Lerp(
                Target.transform.position,
                source.transform.position,
                t / pullDuration
            );
            
            line.SetPosition(1, newPosition);

            Target.transform.position = newPosition;

            yield return null;
        }

        Target.transform.position = source.transform.position;

        Target.moveable = true;
        source.moveable = true;

        line.gameObject.Destroy();
        yield break;
    }

    [MethodRpc((uint)RPC.ChangeBodyType)]
    public static void RpcSetBodyType(this PlayerControl target, PlayerBodyTypes type)
    {
        target.MyPhysics.SetBodyType(type);
        if (type == PlayerBodyTypes.Seeker)
        {
            target.cosmetics.SetBodyCosmeticsVisible(false);
        }
        if (type == PlayerBodyTypes.Long || type == PlayerBodyTypes.LongSeeker)
        {
            target.cosmetics.ShowLongModeParts(true);
        }
        else target.cosmetics.SetBodyCosmeticsVisible(true);
    }

    [MethodRpc((uint)RPC.LightUp)]
    public static void RpcPlaceLantern(this PlayerControl source)
    {
        if (source.Data.Role is LightenerRole light)
        {
            var L = Object.Instantiate(Assets.LanternObject.LoadAsset());
            L.transform.position = new(source.transform.position.x, source.transform.position.y, source.transform.position.y / 1000);
            Lantern lantern = L.AddComponent<Lantern>();
            lantern.LightRadius = OptionGroupSingleton<LightenerOptions>.Instance.LightRadius.Value;
        }
    }
    
    [MethodRpc((uint)RPC.Act)]
    public static void RpcAct(this PlayerControl Source, PlayerControl Target, int gain)
    {
        if (Target.HasModifier<ActModifier>()) Target.GetModifier<ActModifier>().OnAct(gain);
        else Target.AddModifier<ActModifier>(Source).OnAct(gain);
    }

    [MethodRpc((uint)RPC.Vacuum)]
    public static void RpcVacuum(this PlayerControl source)
    {
        foreach (var p in Helpers.GetClosestPlayers(source, 3000f).Where(x => x.Data.IsDead && !x.HasModifier<AbsorbedModifier>()))
        {
            Coroutines.Start(CoVacuum(p, source));
        }
    }
    public static  IEnumerator CoVacuum(PlayerControl toBeAbsorbed, PlayerControl source)
    {
        source.StartCoroutine(Effects.Slide2D(toBeAbsorbed.transform, toBeAbsorbed.transform.position, source.transform.position, Mathf.Clamp(Vector3.Distance(source.transform.position, toBeAbsorbed.transform.position)/2, 0.3f, 1f)));
        Vector3 prevSize = toBeAbsorbed.transform.localScale;
        source.StartCoroutine(Effects.ScaleIn(toBeAbsorbed.transform, prevSize.x, 0, 1.5f));
        
        if (source.AmOwner)
        {
            for (float t = 0; t < 1.5f; t += Time.deltaTime)
            {
                toBeAbsorbed.Visible = true;
                yield return null;
            }
            toBeAbsorbed.Visible = false;
            toBeAbsorbed.AddModifier<AbsorbedModifier>(source);
        }

        toBeAbsorbed.transform.localScale = prevSize;
        source.Data.Role.TryCast<GhostBusterRole>().AddAbsorbedPlayer(toBeAbsorbed);
    }

    [MethodRpc((uint)RPC.TriggerGhostTrap)]
    public static void RpcTriggerGhostTrap(this PlayerControl target, int id)
    {
        GhostTrap trap = GhostTrap.allGhostTraps.First(x => x.id == id);
        trap.isAnimating = true;
        Coroutines.Start(trap.CoAnimateTrapped(target));
    }
    
    [MethodRpc((uint)RPC.PlaceGhostTrap)]
    public static void RpcPlaceGhostTrap(this PlayerControl source)
    {
        GhostTrap trap = new GameObject("Trap").AddComponent<GhostTrap>();
        trap.transform.position = source.GetTruePosition();
        trap.isAnimating = false;
        trap.source = source;
        trap.gameObject.AddComponent<SpriteRenderer>().sprite = Assets.GhostTrap.LoadAsset();
        trap.gameObject.layer = LayerMask.NameToLayer("Objects");
        Coroutines.Start(RFSEffects.Boop(trap.transform, 1.75f, 0.5f, 0.1f));
    }
    
    [MethodRpc((uint)RPC.UseAbility)]
    public static void RpcUseAbility(this PlayerControl source)
    {
        PlayerControl.LocalPlayer.GetModifierComponent().TryGetModifier(out ParanoidModifier modifier);
        modifier?.ShowIndicator(source);
    }
    
    [MethodRpc((uint)RPC.Silence)]
    public static void RpcSilence(this PlayerControl source)
    {
        var lp = PlayerControl.LocalPlayer;
        PlayerTask.GetOrCreateTask<SilenceTask>(lp, 0);
    }
    
    [MethodRpc((uint)RPC.Revive)]
    public static void RpcRevive(this PlayerControl source, PlayerControl target)
    {
        Coroutines.Start(CoRevive(source, target));
    }

    private static IEnumerator CoRevive(PlayerControl source, PlayerControl target)
    {
        target.gameObject.SetActive(false);
        if (target.AmOwner)
        {
            HudManager.Instance.StartCoroutine(Effects.Slide2DWorld(Camera.main.transform, Camera.main.transform.position,
                source.transform.position, 1));
        }
        yield return new WaitForSeconds(1.5f);
        target.NetTransform.SnapTo(source.GetTruePosition());
        target.gameObject.SetActive(true);
        target.Revive();
        if (target.AmOwner || source.AmOwner) HudManager.Instance.FadeScreen(Color.green, Color.green.ToClearColor(), 0.4f);
        yield break;
    }
    
    [MethodRpc((uint)RPC.Sleep)]
    public static void RpcPacifyPlayers(this PlayerControl source)
    {
        //Smoke cloud effect
        var cloudsParent = new GameObject("PacifyCloud");
        cloudsParent.transform.position = source.transform.position;
        for (int i = 0; i < 3; i++)
        {
            var rend = new GameObject("CloudRend").AddComponent<SpriteRenderer>();
            rend.transform.localScale = Vector3.one * OptionGroupSingleton<SleepcasterOptions>.Instance.AbilityRange.Value;
            rend.transform.parent = cloudsParent.transform;
            rend.sprite = Assets.Cloud.LoadAsset();
            source.StartCoroutine(Effects.Slide2D(rend.transform, Vector3.zero,
                new(UnityRandom.RandomRange(-2, 2), UnityRandom.RandomRange(-2, 2)), 1.2f));
            Coroutines.Start(RFSEffects.ColorFadeAndDestroy(rend, Color.blue.LightenColor(0.6f), Color.blue.ToClearColor(), 1.3f));
        }

        source.StartCoroutine(Effects.ActionAfterDelay(3, new System.Action(() =>
        {
            cloudsParent.Destroy();
        })));
        
        //Modifier assignment logic
        foreach (var p in Helpers.GetClosestPlayers(source, 3f * OptionGroupSingleton<SleepcasterOptions>.Instance.AbilityRange.Value))
        {
            p.AddModifier<SleepyModifier>();
        }
    }

    [MethodRpc((uint)RPC.UseWildCard)]
    public static void RpcUseWildCard(this PlayerControl source, byte TargetId, uint Carduint)
    {
        var card = (WildcardDeck.Cards)Carduint;
        var p = PlayerControlUtils.GetPlayerById(TargetId);
        var voteData = p.GetVoteData();
        Color color = Color.white;
        PluginSingleton<StargazerPlugin>.Instance.Log.LogInfo(source.gameObject.name + " used card" + card.ToString() + " on " + p.gameObject.name);
        switch (card)
        {
            case WildcardDeck.Cards.Block:
                voteData.Votes.Clear();
                voteData.SetRemainingVotes(0);
                color = Color.red;
                break;
            case WildcardDeck.Cards.PlusTwo:
                voteData.VoteForPlayer(TargetId);
                voteData.VoteForPlayer(TargetId);
                color = Color.cyan;
                break;
            case WildcardDeck.Cards.PlusFour:
                voteData.VoteForPlayer(TargetId);
                voteData.VoteForPlayer(TargetId);
                voteData.VoteForPlayer(TargetId);
                voteData.VoteForPlayer(TargetId);
                color = Color.magenta.DarkenColor();
                break;
            case WildcardDeck.Cards.Cosmetic:
                p.MixUpOutfit(PlayerControl.AllPlayerControls.ToArray().Random().CurrentOutfit);
                color = Color.magenta;
                break;
            case WildcardDeck.Cards.Mute:
                if (p.AmOwner)
                {
                    HudManager.Instance.Chat.gameObject.SetActive(false);
                    color = Color.red;
                }
                return;
        }

        var voteArea = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == TargetId);
        var rend = new GameObject("Rend").AddComponent<SpriteRenderer>();
        rend.sprite = Assets.Circle.LoadAsset();
        rend.color = color;
        rend.transform.position = voteArea.PlayerIcon.transform.position;
        rend.gameObject.layer = LayerMask.NameToLayer("UI");
        MeetingHud.Instance.StartCoroutine(Effects.Bloop(0, rend.transform, 1, 0.75f));
        Coroutines.Start(RFSEffects.ColorFadeAndDestroy(rend, color, color.ToClearColor(), 1f));
    }

    [MethodRpc((uint)RPC.DisconnectVent)]
    public static void RpcDisconnectVent(this PlayerControl source, int ventId, int btnIndex)
    {
        var vent = Helpers.GetVentById(ventId);
        if (!vent) return;
        var btn = vent.Buttons[btnIndex];
        if (!btn) return;
        btn.spriteRenderer.enabled = false;
        btn.colliders[0].enabled = false;
        btn.GetComponent<BoxCollider2D>().enabled = false;
        btn.enabled = false;

        var linkedVents = new []{ vent.Right, vent.Left, vent.Center };
        var ventTarget = linkedVents[btnIndex]; //Target vent on other side
        var linkedVentsTarget = new []{ ventTarget.Right, ventTarget.Left, ventTarget.Center };
        int btnIndexTarget = linkedVentsTarget.ToList().IndexOf(vent);
        var btnTarget = ventTarget.Buttons[btnIndexTarget];
        btnTarget.spriteRenderer.enabled = false;
        btnTarget.GetComponent<BoxCollider2D>().enabled = false;
        btnTarget.enabled = false;
    }
    
    [MethodRpc((uint)RPC.MoveFloristControlledPlayer)]
    public static void RpcMoveFloristControlledPlayer(this PlayerControl source, byte targetId, float x, float y)
    {
        var target = PlayerControlUtils.GetPlayerById(targetId);
        if (target == null || target.Data == null || target.Data.IsDead)
        {
            return;
        }

        if (!target.HasModifier<ControlledByFloristModifier>())
        {
            return;
        }

        Vector2 velocity = new Vector2(x, y);

        target.MyPhysics.body.velocity = velocity;
        target.MyPhysics.HandleAnimation(target.Data.IsDead);
    }
    
    [MethodRpc((uint)RPC.SpawnFloristTrap)]
    public static void RpcSpawnFloristTrap(this PlayerControl source, uint id)
    {
        FloristRole.FlowerTypes type = (FloristRole.FlowerTypes)id;

        GameObject obj = null;
        PlayerDetectionBehaviour playerDetectionBehaviour = null;
        bool fade = true;
        switch (type)
        {
            case FloristRole.FlowerTypes.TallGrass:
                obj = UnityObject.Instantiate(Assets.FloristTallGrass.LoadAsset());
                obj.transform.position = source.transform.position;
                playerDetectionBehaviour = obj.AddComponent<PlayerDetectionBehaviour>();
                playerDetectionBehaviour.Radius = new(2, 1.5f);
                playerDetectionBehaviour.OnEnter = control =>
                {
                    control.AddModifier<SlowedDownModifier>();
                };
                playerDetectionBehaviour.OnExit = control =>
                {
                    control.RemoveModifier<SlowedDownModifier>();
                };
                break;
            case FloristRole.FlowerTypes.Flowers:
                obj = UnityObject.Instantiate(Assets.FloristFlowers.LoadAsset());
                obj.transform.position = source.transform.position;

                PlayerMaterial.SetColors(FloristRole.FlowerColors.Random(), obj.GetComponent<SpriteRenderer>());

                playerDetectionBehaviour = obj.AddComponent<PlayerDetectionBehaviour>();
                playerDetectionBehaviour.LocalOffset = new Vector2(-0.2f, -0.7f);
                // playerDetectionBehaviour.Size = new Vector2(2.7f, 1.5f);

                playerDetectionBehaviour.LocalOffset = new Vector2(-0.2f, -0.7f);
                playerDetectionBehaviour.Radius = new Vector2(1.35f, 0.75f);

                // DebugRadius.CreateCircle(
                //     obj.transform,
                //     playerDetectionBehaviour.LocalOffset,
                //     playerDetectionBehaviour.Radius,
                //     999
                // );

                playerDetectionBehaviour.OnEnter = control =>
                {
                    if (!control.HasModifier<BlossomModifier>())
                    {
                        control.AddModifier<BlossomModifier>();
                    }
                };

                playerDetectionBehaviour.OnStay = control =>
                {
                    if (control.TryGetModifier<BlossomModifier>(out BlossomModifier m))
                    {
                        m.IncreaseBlossom();
                    }
                };
                break;
            case FloristRole.FlowerTypes.Thorns:
                fade = false;
                obj = UnityObject.Instantiate(Assets.FloristThorns.LoadAsset());
                obj.GetComponent<SpriteRenderer>().enabled = false;
                obj.transform.position = source.transform.position;
                obj.transform.localScale = Vector3.one / 2;
                var indicator = new GameObject("Indicator").AddComponent<SpriteRenderer>();
                indicator.sprite = Assets.ThornsIndicator.LoadAsset();
                indicator.transform.position = source.transform.position;
                indicator.transform.localScale = Vector3.one / 2;
                indicator.color = new(1, 1, 1, 0.3f);
                playerDetectionBehaviour = obj.AddComponent<PlayerDetectionBehaviour>();
                playerDetectionBehaviour.enabled = false;
                source.StartCoroutine(Effects.ActionAfterDelay(1f, new System.Action(() => { playerDetectionBehaviour.enabled = true; })));
                playerDetectionBehaviour.OnEnter = control =>
                {
                    control.StartCoroutine(Effects.ActionAfterDelay(0.3f, new System.Action(() =>
                    {
                        obj.GetComponent<SpriteRenderer>().enabled = true;
                        indicator.color = new(1, 1, 1, 1f);
                        if (Vector2.Distance(control.transform.position, source.transform.position) <= 0.2f)
                        {
                            control.CustomMurder(control, MurderResultFlags.Succeeded, showKillAnim:false);
                        }
                        source.StartCoroutine(Effects.ActionAfterDelay(0.7f, new System.Action(() => { obj.Destroy(); })));
                    })));
                };
                break;
            case FloristRole.FlowerTypes.Mushroom:
                fade = false;
                var mushroom = UnityObject.Instantiate(MapLoader.Fungle.GetComponentInChildren<Mushroom>());
                mushroom.transform.position = source.transform.position;
                mushroom.origPosition = source.transform.position;
                mushroom.enabled = false;
                source.StartCoroutine(Effects.ActionAfterDelay(1f, new System.Action(() => { mushroom.enabled = true; })));
                break;
        }

        if (playerDetectionBehaviour == null || obj == null || !fade) return;
        playerDetectionBehaviour.OnEnter += control =>
        {
            control.StartCoroutine(Effects.ColorFade(obj.GetComponent<SpriteRenderer>(), Color.white.ToClearColor(), Color.white, 1f));

        };
        playerDetectionBehaviour.OnExit += control =>
        {
            control.StartCoroutine(Effects.ColorFade(obj.GetComponent<SpriteRenderer>(), Color.white, Color.white.ToClearColor(), 0.4f));
        };
    }

    [MethodRpc((uint)RPC.SwapRoles)]
    public static void RpcSwapRoles(this PlayerControl source, PlayerControl target, uint roleId)
    {
        RoleTypes type = (RoleTypes)roleId;
        if (target.Data.Role.Role == type)
        {
            var ogRole = target.Data.RoleType;
            RoleManager.Instance.SetRole(target, (RoleTypes)RoleId.Get<RolelessRole>());
            RoleManager.Instance.SetRole(source, type);
        }
        else
        {
            if (OptionGroupSingleton<RolelessOptions>.Instance.SuicideWhenMisguess) target.CustomMurder(source, MurderResultFlags.Succeeded, false, true, false, false, true);
        }
    }

    [MethodRpc((uint)RPC.ShootPlayer)]
    public static void RpcShootPlayer(this PlayerControl source, PlayerControl target, int bulletCount)
    {
        if (target.AmOwner) ShotMinigame.CreateAndOpen(bulletCount);
    }
}