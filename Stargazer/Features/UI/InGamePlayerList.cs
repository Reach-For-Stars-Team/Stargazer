using System;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.LocalSettings;
using Stargazer.Options;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Features;

public static class InGamePlayerList
{
    private static global::InGamePlayerList _plist;
    private static bool _isOpen;
    public static PassiveButton PlayerListButton;

    public static void CreateButton()
    {
        _plist = UnityObject.FindFirstObjectByType<global::InGamePlayerList>(FindObjectsInactive.Include);

        PlayerListButton = UnityObject
            .Instantiate(HudManager.Instance.SettingsButton, HudManager.Instance.SettingsButton.transform.parent)
            .GetComponent<PassiveButton>();
        PlayerListButton.gameObject.name = "PlayerListButton";
        PlayerListButton.OnClick = new();
        PlayerListButton.OnClick.AddListener(OnClick());
        PlayerListButton.activeSprites.GetComponent<SpriteRenderer>().sprite = Assets.PListActive.LoadAsset();
        PlayerListButton.inactiveSprites.GetComponent<SpriteRenderer>().sprite = Assets.PListInactive.LoadAsset();
        PlayerListButton.GetComponent<AspectPosition>().DistanceFromEdge = new(2.75f, 0.505f, -400f);
        
        _isOpen = false;
        
        PlayerListButton.gameObject.SetActive(LocalSettingsTabSingleton<ClientOptions>.Instance.EnableInGamePlayerList.Value);
    }

    private static Action OnClick()
    {
        void Listener()
        {
            _plist.SetActive(!_isOpen);
            _isOpen = !_isOpen;
        }

        return Listener;
    }

    [RegisterEvent]
    public static void OnGameStart(IntroBeginEvent e)
    {
        PlayerListButton.gameObject.DestroyImmediate();
        _plist.SetActive(false);
    }

    [RegisterEvent]
    public static void OnPlayerJoined(PlayerJoinEvent e)
    {
        if (_plist) _plist.RefreshMenu();
    }
}