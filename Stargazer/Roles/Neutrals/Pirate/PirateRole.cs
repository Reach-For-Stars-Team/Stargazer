using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using Stargazer.Components;
using Stargazer.Mapping;
using Stargazer.Utilities;
using UnityEngine;

namespace Stargazer.Roles.Neutrals.Pirate;

public class PirateRole : CrewmateRole, INeutralRole
{
    public override bool IsAffectedByComms => false;
    public string RoleName => "Pirate";
    public string RoleDescription => "Admirer Of the Shiny Rock.";
    public string RoleLongDescription => "Collect 1000 gold to win!" +"\nYou can gain gold by:\n" +
                                         "- Doing Tasks\n" +
                                         "- Stealing from players\n" +
                                         "- Looking for treasure";
    public Color RoleColor => RFSPalette.PirateRoleColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public int gold = 0;
    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = false,
        IntroSound = Assets.MoneySfx,
        CanGetKilled = true,
        CanUseVent = false,
        CanUseSabotage = false,
        TasksCountForProgress = false,
        Icon = Assets.PirateRoleIcon
    };

    public void IncreaseGold(int amount)
    {
        gold += amount;
        HudManager.Instance.FadeScreen(Color.yellow, new(1, 1, 0, 0), 0.3f);
        SoundManager.Instance.PlaySound(Assets.MoneySfx.LoadAsset(), false);
        if (gold >= 1000) PlayerControl.LocalPlayer.RpcAddModifier<NeutralWinner>();
    }

    public override void Initialize(PlayerControl p)
    {
        RoleBehaviourStubs.Initialize(this, p);
        SpawnMapPickup();
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return Player.HasModifier<NeutralWinner>();
    }

    public StringBuilder SetTabText()
    {
        return CustomRoleUtils.CreateForRole(this).Append("\n\n\n" +
                                                          $"<color=#{ColorUtility.ToHtmlStringRGBA(Color.yellow)}><b> Gold: {gold} </b></color>");
    }

    private PlayerDetectionBehaviour _mapPlayerDetection;
    private ArrowBehaviour arrow;
    public SpriteRenderer xMark;
    public override void OnVotingComplete()
    {
        SpawnMapPickup();
    }

    public void SpawnMapPickup()
    {
        if (Player.HasModifier<NeutralWinner>() || Player.Data.IsDead || !Player.AmOwner) return;
        if (_mapPlayerDetection) _mapPlayerDetection.gameObject.Destroy();
        _mapPlayerDetection = new GameObject("MapPickup").AddComponent<PlayerDetectionBehaviour>();
        _mapPlayerDetection.canTrigger = true;
        RandomizationUtils.SpawnObjectRandomly(
            _mapPlayerDetection.gameObject,
            new Vector2(1.5f, 1.5f),
            true,
            RandomizationUtils.SpawnAreaFilter.AnyRachable
        );

        _mapPlayerDetection.transform.position = new Vector3(
            _mapPlayerDetection.transform.position.x,
            _mapPlayerDetection.transform.position.y,
            0f
        );
        _mapPlayerDetection.OnEnter = p =>
        {
            _mapPlayerDetection.canTrigger = false;
            HudManager.Instance.SpawnTextOverlay("Treasure Location Revealed!");
            _mapPlayerDetection.gameObject.Destroy();
            if (xMark) xMark.gameObject.Destroy();
            if (arrow) arrow.gameObject.Destroy();
            xMark = new GameObject("xMark").AddComponent<SpriteRenderer>();
            xMark.sprite = Assets.XMark.LoadAsset();
            RandomizationUtils.SpawnObjectRandomly(
            xMark.gameObject,
            new Vector2(1.5f, 1.5f),
            true,
            RandomizationUtils.SpawnAreaFilter.ReachableWitchoutRooms
        );

            xMark.transform.position = new Vector3(
                xMark.transform.position.x,
                xMark.transform.position.y,
                xMark.transform.position.y / 1000f + 0.0009f
            );
            arrow = Helpers.CreateArrow(HudManager.Instance.transform, RFSPalette.PirateRoleColor);
            arrow.target = xMark.transform.position;
        };
        _mapPlayerDetection.gameObject.AddComponent<SpriteRenderer>().sprite = Assets.Map.LoadAsset();
    }

    [RegisterEvent]
    public static void OnTaskComplete(CompleteTaskEvent e)
    {
        if (e.Player.AmOwner && e.Player.Data.Role is PirateRole pirate)
        {
            pirate.IncreaseGold(OptionGroupSingleton<PirateOptions>.Instance.GoldPerTask);
        }
    }
}



[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class DebugMapSpawner
{
    private static readonly List<GameObject> SpawnedObjects = new();

    public static void Postfix()
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost && false) { }

        if (!PlayerControl.LocalPlayer) return;
        if (!HudManager.Instance) return;
        if (!ShipStatus.Instance) return;

        if (PlayerControl.LocalPlayer.Data?.Role is not PirateRole)
            return;

        // if (Input.GetKeyDown(KeyCode.F6))
        // {
        //     SpawnDebugMap();
        // }

        // if (Input.GetKeyDown(KeyCode.F8))
        // {
        //     SpawnDebugXMark();
        // }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            ClearDebugObjects();
        }
    }

    private static void SpawnDebugMap()
    {
        var map = new GameObject("DebugMapSpawn");

        var renderer = map.AddComponent<SpriteRenderer>();
        renderer.sprite = Assets.Map.LoadAsset();

        RandomizationUtils.SpawnObjectRandomly(
            map,
            new Vector2(0.5f, 0.5f),
            true,
            RandomizationUtils.SpawnAreaFilter.AnyRachable
        );

        map.transform.position = new Vector3(
            map.transform.position.x,
            map.transform.position.y,
            map.transform.position.y / 1000f - 0.0009f
        );

        SpawnedObjects.Add(map);

        HudManager.Instance.SpawnTextOverlay($"Debug objects: {SpawnedObjects.Count}");
    }

    private static void SpawnDebugXMark()
    {
        var xMark = new GameObject("DebugXMarkSpawn");

        var renderer = xMark.AddComponent<SpriteRenderer>();
        renderer.sprite = Assets.XMark.LoadAsset();

        RandomizationUtils.SpawnObjectRandomly(
            xMark,
            new Vector2(1.0f, 1.0f),
            true,
            RandomizationUtils.SpawnAreaFilter.ReachableWitchoutRooms
        );

        xMark.transform.position = new Vector3(
            xMark.transform.position.x,
            xMark.transform.position.y,
            1f
        );

        SpawnedObjects.Add(xMark);

        HudManager.Instance.SpawnTextOverlay($"Debug objects: {SpawnedObjects.Count}");
    }

    private static void ClearDebugObjects()
    {
        foreach (var obj in SpawnedObjects)
        {
            if (obj) obj.Destroy();
        }

        SpawnedObjects.Clear();

        HudManager.Instance.SpawnTextOverlay("Debug objects cleared");
    }
}
