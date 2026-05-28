using System.Linq;
using System.Text;
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
        RandomizationUtils.SpawnObjectRandomly(_mapPlayerDetection.gameObject, new(1.5f, 1.5f));
        _mapPlayerDetection.transform.position = new Vector3(_mapPlayerDetection.gameObject.transform.position.x, _mapPlayerDetection.gameObject.transform.position.y, 0);
        _mapPlayerDetection.OnEnter = p =>
        {
            _mapPlayerDetection.canTrigger = false;
            HudManager.Instance.SpawnTextOverlay("Treasure Location Revealed!");
            _mapPlayerDetection.gameObject.Destroy();
            if (xMark) xMark.gameObject.Destroy();
            if (arrow) arrow.gameObject.Destroy();
            xMark = new GameObject("xMark").AddComponent<SpriteRenderer>();
            xMark.sprite = Assets.XMark.LoadAsset();
            RandomizationUtils.SpawnObjectRandomly(xMark.gameObject, new(2f, 2f));
            xMark.transform.position = new Vector3(xMark.gameObject.transform.position.x, xMark.gameObject.transform.position.y, 1);
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