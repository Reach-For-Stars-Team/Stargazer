using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TMPro;
using UnityEngine;

namespace Stargazer;

public static class Assets
{
    public static readonly AssetBundle Bundle = AssetBundleManager.Load("rfsbundle");

    public static LoadableBundleAsset<GameObject> CreditsMenuPrefab = new("CreditsMenuPrefab.prefab", Bundle);

    public static LoadableResourceAsset PListActive = new("Stargazer.Resources.UI.PlayerListActive.png");
    public static LoadableResourceAsset PListInactive = new("Stargazer.Resources.UI.PlayerListInactive.png");

    public static LoadableResourceAsset Square { get; } =
        new("Stargazer.Resources.UI.Square.png");
    public static LoadableResourceAsset Circle { get; } = 
        new("Stargazer.Resources.UI.Circle.png", 75);
    public static LoadableResourceAsset PlaceHolder { get; } = new("Stargazer.Resources.PlaceHolder.png");

    public static LoadableResourceAsset AdminButton { get; } = new("Stargazer.Resources.Abilities.AdminButton.png");
    
    //Jester
    public static LoadableAudioResourceAsset JesterIntroSfx { get; } =
        new("Stargazer.Resources.SoundEffects.JesterIntro.wav");

    public static LoadableResourceAsset JesterIcon { get; } = new("Stargazer.Resources.RoleIcons.jester.png");


    //Mole
    public static LoadableBundleAsset<AnimationClip> DigAnimation { get; } =
        new("DigAnimation.anim", Bundle);
    public static LoadableResourceAsset DigButton { get; } = new("Stargazer.Resources.Abilities.DigButton.png");
    public static LoadableAudioResourceAsset DigSfx { get; } = new("Stargazer.Resources.SoundEffects.Dig.wav");
    
    //Lightener
    public static LoadableBundleAsset<GameObject> LanternObject { get; } = new("lanternPrefab.prefab", Bundle);

    public static LoadableBundleAsset<AnimationClip> LanternBobAnim { get; } =
        new("LanternBobbingAnimation.anim", Bundle);
    public static LoadableResourceAsset LightUpButton { get; } = new("Stargazer.Resources.Abilities.LightUpButton.png");

    public static LoadableAudioResourceAsset ChainsSfx { get; } = new("Stargazer.Resources.SoundEffects.Chain.wav");
    
    public static LoadableAudioResourceAsset WildcardIntro { get; } = new("Stargazer.Resources.SoundEffects.WildcardIntro.wav");
    
    public static LoadableAudioResourceAsset SpyIntro { get; } = new("Stargazer.Resources.SoundEffects.SpyIntro.wav");

    public static LoadableAudioResourceAsset VacuumGhostSfx { get; } =
        new("Stargazer.Resources.SoundEffects.VacuumGhost.wav");
    
    public static LoadableBundleAsset<GameObject> StalkMinigame { get; } = new("StalkerTabletPrefab.prefab", Bundle);
    public static LoadableBundleAsset<RenderTexture> StalkCamTex { get; } = new("StalkedCamTex.renderTexture", Bundle);
    public static LoadableBundleAsset<GameObject> RfsLogo { get; } = new("RFSLogo.prefab", Bundle);
    public static LoadableAudioResourceAsset MoneySfx { get; } = new("Stargazer.Resources.SoundEffects.Money.wav");
    public static LoadableResourceAsset HandHoldingBody { get; } = new("Stargazer.Resources.Objects.HandHoldingBody.png");
    public static LoadableResourceAsset DousedOverlay { get; } = new("Stargazer.Resources.UI.DousedOverlay.png", 548);
    public static LoadableBundleAsset<RuntimeAnimatorController> BurnButtonAnimationController =
        new("burnButtonAnimationController.controller", Bundle);
    
    public static readonly LoadableBundleAsset<GameObject> flamePrefab =
        new("flamePrefab.prefab", Bundle);

    public static readonly LoadableBundleAsset<AnimationClip> wrangledPlayerWalkAnim =
        new LoadableBundleAsset<AnimationClip>("WrangledPlayerWalk.anim", Bundle);
    
    public static readonly LoadableBundleAsset<Material> ropeMaterial =
        new LoadableBundleAsset<Material>("RopeMaterial.mat", Bundle);
    public static LoadableResourceAsset LassoButton { get; } = new("Stargazer.Resources.Abilities.LassoButton.png"); 
    
    public static readonly LoadableBundleAsset<Material> GrayscaleMaterial =
        new LoadableBundleAsset<Material>("Custom_DesaturationShader.mat", Bundle);

    public static LoadableResourceAsset SilenceButton { get; } = new("Stargazer.Resources.Abilities.SilenceButton.png"); 
    public static LoadableResourceAsset ActorRoleIcon { get; } = new("Stargazer.Resources.RoleIcons.actor.png");
    public static LoadableResourceAsset StalkButton { get; } = new("Stargazer.Resources.Abilities.StalkButton.png");
    public static LoadableResourceAsset WatchButton { get; } = new("Stargazer.Resources.Abilities.WatchButton.png");
    public static LoadableResourceAsset SpyRoleIcon { get; } = new("Stargazer.Resources.RoleIcons.spy.png");
    public static LoadableResourceAsset ObserverRoleIcon { get; } = new("Stargazer.Resources.RoleIcons.observer.png");
    public static LoadableResourceAsset ObserveButton { get; } = new("Stargazer.Resources.Abilities.ObserveButton.png");
    public static LoadableResourceAsset ActButton { get; } = new("Stargazer.Resources.Abilities.ActButton.png");
    public static LoadableResourceAsset Map { get; } = new("Stargazer.Resources.Objects.Map.png"); 
    public static LoadableResourceAsset ThornsIndicator { get; } = new("Stargazer.Resources.Objects.ThornsIndicator.png"); 
    public static LoadableResourceAsset PlantFlowersButton { get; } = new("Stargazer.Resources.Abilities.PlantFlowersButton.png"); 
    public static LoadableResourceAsset PlantGrassButton { get; } = new("Stargazer.Resources.Abilities.PlantGrassButton.png");
    public static LoadableResourceAsset PlantMushroomButton { get; } = new("Stargazer.Resources.Abilities.PlantMushroomButton.png"); 
    public static LoadableResourceAsset FlowerTakeoverSprite { get; } = new("Stargazer.Resources.Objects.FlowerTakeover.png"); 
    public static LoadableResourceAsset PlantThornsButton { get; } = new("Stargazer.Resources.Abilities.PlantThornsButton.png"); 
    public static LoadableResourceAsset XMark { get; } = new("Stargazer.Resources.Objects.XMark.png"); 
    public static LoadableResourceAsset MoleRoleIcon { get; } = new("Stargazer.Resources.RoleIcons.mole.png");
    public static LoadableResourceAsset SilencerRoleIcon { get; } = new("Stargazer.Resources.RoleIcons.silencer.png");
    public static LoadableResourceAsset PirateDigButton { get; } = new("Stargazer.Resources.Abilities.DigButtonPirate.png");
    
    public static LoadableResourceAsset SpiritRoleIcon { get; } = new("Stargazer.Resources.RoleIcons.spirit.png");
    public static LoadableResourceAsset AnalystRoleIcon { get; } = new("Stargazer.Resources.RoleIcons.analyst.png");
    public static LoadableResourceAsset SleepcasterRoleIcon { get; } = new("Stargazer.Resources.RoleIcons.sleepcaster.png");
    public static LoadableResourceAsset PhaserRoleIcon { get; } = new("Stargazer.Resources.RoleIcons.phaser.png");
    public static LoadableResourceAsset GhostFormButton { get; } = new("Stargazer.Resources.Abilities.GhostFormButton.png");
    public static LoadableResourceAsset DouseButton { get; } = new("Stargazer.Resources.Abilities.DouseButton.png");
    public static LoadableResourceAsset GogglesButton { get; } = new("Stargazer.Resources.Abilities.GogglesButton.png");
    public static LoadableResourceAsset VacuumButton { get; } = new("Stargazer.Resources.Abilities.VacuumButton.png");

    public static LoadableResourceAsset CowboyRoleIcon { get; set; } =
        new("Stargazer.Resources.RoleIcons.cowboy.png");
    
    public static LoadableResourceAsset PyromaniacRoleIcon { get; set; } =
        new("Stargazer.Resources.RoleIcons.pyromaniac.png");
    //Pirate:
    public static LoadableResourceAsset PirateRoleIcon { get; set; } =
        new("Stargazer.Resources.RoleIcons.pirate.png");
    public static LoadableResourceAsset StealButton { get; set; } =
        new("Stargazer.Resources.Abilities.StealButton.png");
    //Ghost buster:
    public static LoadableResourceAsset GhostbusterRoleIcon { get; } =
        new("Stargazer.Resources.RoleIcons.ghostbuster.png");
    public static LoadableResourceAsset GhostTrap { get; } =
        new("Stargazer.Resources.Objects.GhostTrap.png");
    public static LoadableResourceAsset GogglesOverlay { get; } = 
        new("Stargazer.Resources.UI.GogglesOverlay.png", 548);
    public static LoadableResourceAsset Wind { get; } =
        new("Stargazer.Resources.Objects.Wind.png");
    //Taskmaster:
    public static LoadableResourceAsset TaskmasterRoleIcon { get; } =
        new("Stargazer.Resources.RoleIcons.taskmaster.png");
    public static LoadableResourceAsset DoTaskButton { get; } =
        new("Stargazer.Resources.Abilities.DoTaskButton.png");
    //Paranoiac:
    public static LoadableResourceAsset ParanoiacRoleIcon { get; } =
        new("Stargazer.Resources.RoleIcons.paranoiac.png");
    public static LoadableResourceAsset ParanoiaButton { get; } =
        new("Stargazer.Resources.Abilities.ParanoiaButton.png");
    public static LoadableResourceAsset AbilityUsedVisual { get; } = 
        new("Stargazer.Resources.UI.AbilityUsedVisual.png", 75);
    //Carrier
    public static LoadableResourceAsset CarrierRoleIcon { get; } =
        new("Stargazer.Resources.RoleIcons.carrier.png");
    //Lightener
    public static LoadableResourceAsset LightenerRoleIcon { get; } =
        new("Stargazer.Resources.RoleIcons.lightener.png");
    
    //Sleepcaster
    public static LoadableBundleAsset<GameObject> SleepOverlay =
        new LoadableBundleAsset<GameObject>("SleepOverlay.prefab", Bundle);
    
    public static LoadableBundleAsset<AnimationClip> SleepingPlayerAnimation =
        new LoadableBundleAsset<AnimationClip>("SleepingPlayerAnimation.anim", Bundle);
    public static LoadableResourceAsset Cloud { get; } = new("Stargazer.Resources.Objects.Cloud.png");
    public static LoadableResourceAsset PacifyButton { get; } = new("Stargazer.Resources.Abilities.PacifyButton.png");
    public static LoadableResourceAsset ReviveButton { get; } = new("Stargazer.Resources.Abilities.ReviveButton.png");
    public static LoadableResourceAsset HandHoldingTorch { get; } = new("Stargazer.Resources.Objects.HandHoldingTorch.png");
    public static LoadableResourceAsset AnalyzeButton { get; set; } =
        new("Stargazer.Resources.Abilities.AnalyzeButton.png");
    
    public static LoadableAudioResourceAsset TeleportSfx { get; } =
        new("Stargazer.Resources.SoundEffects.Teleport.wav");
    public static LoadableResourceAsset WarpButton = new("Stargazer.Resources.Abilities.WarpButton.png");
    public static LoadableBundleAsset<GameObject> ShurikenProjectilePrefab =
        new LoadableBundleAsset<GameObject>("ShurikenProjectilePrefab.prefab", Bundle);
    
    public static LoadableResourceAsset PhaseTarget { get; } = 
        new("Stargazer.Resources.UI.PhaseTarget.png", 128);
    public static LoadableResourceAsset Target { get; } = 
        new("Stargazer.Resources.UI.Target.png", 75);
    //Wildcard
    public static LoadableBundleAsset<GameObject> WildcardDeckPrefab = new("WildcardCardDeckCanvas.prefab", Bundle);
    public static LoadableBundleAsset<GameObject> ChestPopoutPrefab = new("ChestPopout.prefab", Bundle);
    public static LoadableResourceAsset CardsButton { get; } = new("Stargazer.Resources.Abilities.CardsButton.png");
    public static LoadableResourceAsset DisconnectButton { get; } = new("Stargazer.Resources.Abilities.DisconnectButton.png");
    public static LoadableResourceAsset MechanicRoleIcon { get; } = new("Stargazer.Resources.RoleIcons.mechanic.png");
    public static LoadableResourceAsset WildcardRoleIcon { get; } =  new("Stargazer.Resources.RoleIcons.wildcard.png");
    public static LoadableBundleAsset<GameObject> FloristTallGrass { get; } = new("FloristTallGrass.prefab", Bundle);
    public static LoadableBundleAsset<GameObject> FloristThorns { get; } = new("FloristThorns.prefab", Bundle);
    public static LoadableBundleAsset<GameObject> ControlledOverlay { get; } = new("FloristControlledCanvas.prefab", Bundle);
    public static LoadableBundleAsset<GameObject> FloristFlowers { get; } = new("FloristFlowers.prefab", Bundle);
}