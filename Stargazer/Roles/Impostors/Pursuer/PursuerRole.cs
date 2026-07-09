using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Impostors.Pursuer;

[MiraIgnore]
public class PursuerRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Pursuer";

    public string RoleDescription => "throw new System.NotImplementedException()";

    public string RoleLongDescription => "throw new System.NotImplementedException()";

    public Color RoleColor => Palette.ImpostorRoleRed;
    
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = false
    };

    public enum PursuitMode
    {
        Single,
        Multiple
    }
    
    public PursuitMode Mode = PursuitMode.Single;
    public override void OnMeetingStart()
    {
        if (!Player.AmOwner) return;
        if (PlayerControl.LocalPlayer.Data.Role is not PursuerRole p) return;
        
        var toggleButton = Object.Instantiate(MeetingHud.Instance.MeetingAbilityButton, MeetingHud.Instance.transform);
        toggleButton.gameObject.SetActive(true);
        toggleButton.buttonLabelText.text = $"Kill Mode:\n{Mode}";
        toggleButton.SetInfiniteUses();
        var pos = toggleButton.gameObject.AddComponent<AspectPosition>();
        pos.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        pos.DistanceFromEdge = Vector3.one;
        pos.AdjustPosition();
        toggleButton.graphic.sprite = Assets.CardsButton.LoadAsset();
        toggleButton.GetComponent<PassiveButton>().OnClick.AddListener(new System.Action(() =>
        {
            if (p.Mode == PursuitMode.Multiple) p.Mode = PursuitMode.Single;
            else p.Mode = PursuitMode.Multiple;
            
            toggleButton.buttonLabelText.text = $"Kill Mode:\n{Mode}";
        }));
    }
}