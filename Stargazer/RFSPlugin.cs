using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using MiraAPI;
using MiraAPI.PluginLoading;
using Stargazer.Components;
using Stargazer.Roles.Crewmates.Lightener;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine.EventSystems;

namespace Stargazer;

[BepInAutoPlugin("com.missingpixel.stargazer", "Stargazer", "2.0.1")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class StargazerPlugin : BasePlugin, IMiraPlugin
{
    public Harmony Harmony { get; } = new(Id);
    public string OptionsTitleText => "Stargazer";

    public ConfigFile GetConfigFile()
    {
        return Config;
    }

    public override void Load()
    {
        Harmony.PatchAll();
        ReactorCredits.Register<StargazerPlugin>(ReactorCredits.AlwaysShow);
        ClassInjector.RegisterTypeInIl2Cpp<PlayerDetectionBehaviour>();
        ClassInjector.RegisterTypeInIl2Cpp<Lantern>();
        ClassInjector.RegisterTypeInIl2Cpp<SwingingLantern>();
        ClassInjector.RegisterTypeInIl2Cpp<CardReleaseZone>(new RegisterTypeOptions()
        {
            Interfaces = new([typeof(IDragHandler), typeof(IBeginDragHandler), typeof(IEndDragHandler)])
        });
        ClassInjector.RegisterTypeInIl2Cpp<CardBehaviour>(new RegisterTypeOptions()
        {
            Interfaces = new Il2CppInterfaceCollection(new[]
            {
                typeof(IDragHandler), typeof(IBeginDragHandler), typeof(IEndDragHandler)
            })
        });
        ClassInjector.RegisterTypeInIl2Cpp<WildcardDeck>();
        if (!Directory.Exists($"{Paths.GameRootPath}/Screenshots"))
        {
            Directory.CreateDirectory($"{Paths.GameRootPath}/Screenshots");
        }
        Log.LogInfo("Stargazer Loaded Successfully! >u<");
    }
}