using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using Microsoft.VisualBasic;
using MiraAPI;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.LocalSettings;
using MiraAPI.LocalSettings.Attributes;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Stargazer.Features;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using Stargazer.Features.Roles;
using UnityEngine;
using UnityEngine.Events;

namespace Stargazer.Options;

public class ClientOptions(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "Stargazer";
        
    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = MiraAssets.SettingsIcon,
        TabColor = RFSPalette.RfsColor2,
        TabButtonColor = RFSPalette.RfsColor,
        TabButtonActiveColor = RFSPalette.RfsColor2,
        TabButtonHoverColor = RFSPalette.RfsColor2,
    };
    
    [LocalToggleSetting]
    public ConfigEntry<bool> ShowLeaveConfirmationPopup { get; private set; } = config.Bind("Menus", "Show confirmation pop-up when exiting game", false);
    
    [LocalToggleSetting]
    public ConfigEntry<bool> EnableInGamePlayerList { get; private set; } = config.Bind("Menus", "Enable In-Game Player list while in lobby", true);

    [LocalToggleSetting]
    public ConfigEntry<bool> EnableBetterMinimap { get; private set; } = config.Bind("Menus", "Enable Better Minimap", true);
    
    [LocalToggleSetting]
    public ConfigEntry<bool> EnableRoleIcons { get; private set; } = config.Bind("Roles", "Enable Role Icons next to player name", true);

    public override void OnOptionChanged(ConfigEntryBase configEntry)
    {
        if (configEntry == EnableInGamePlayerList)
        {
            if (Features.InGamePlayerList.PlayerListButton)
            {
                Features.InGamePlayerList.PlayerListButton.gameObject.SetActive(EnableInGamePlayerList.Value);
            }
        }
    }
}