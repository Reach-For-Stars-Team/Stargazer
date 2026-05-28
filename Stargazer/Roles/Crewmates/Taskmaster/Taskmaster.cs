using MiraAPI.Hud;
using MiraAPI.Roles;
using PowerTools;
using Stargazer.Components;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Roles.Crewmates.Taskmaster;

public class TaskmasterRole : CrewmateGhostRole, ICustomRole
{
    private SwingingLantern lantern;

    public override bool IsAffectedByComms => false;

    public void Start()
    {
        if (Player == null) return;

        lantern = new GameObject("HandAnimation").AddComponent<SwingingLantern>();
        lantern.gameObject.layer = LayerMask.NameToLayer("Ghosts");
        lantern.gameObject.AddComponent<Animator>();
        lantern._animator = lantern.gameObject.AddComponent<SpriteAnim>();
        lantern._renderer = lantern.gameObject.AddComponent<SpriteRenderer>();
        lantern._renderer.material = new Material(Shader.Find("Unlit/PlayerShader"));
        PlayerMaterial.SetColors(Player.cosmetics.ColorId, lantern._renderer);
        lantern.transform.SetParent(Player.MyPhysics.Animations.transform);
        lantern.transform.localPosition = new Vector3(0f, -0.2f, -10f);
        lantern.transform.localScale = new Vector3(0.5f, 0.5f, 10f);
        lantern.Player = Player;

        if (PlayerControl.LocalPlayer == Player) CustomButtonSingleton<TaskAbility>.Instance.SetActive(true, this);
    }

    public string RoleName => "Taskmaster";
    public string RoleLongDescription => "Help players do their tasks!";
    public string RoleDescription => RoleLongDescription;
    public Color RoleColor => RFSPalette.TaskmasterRoleColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.TaskmasterRoleIcon,
        ShowInFreeplay = true,
        HideSettings = false
    };

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        lantern.gameObject.DestroyImmediate();
    }
}