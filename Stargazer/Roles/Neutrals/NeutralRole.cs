using MiraAPI.Modifiers;
using MiraAPI.Roles;
using UnityEngine;

namespace Stargazer.Roles.Neutrals;

public interface INeutralRole : ICustomRole
{
    TeamIntroConfiguration? ICustomRole.IntroConfiguration => new TeamIntroConfiguration(RFSPalette.NeutralGrayColor, "Neutral", "Achieve a specific goal to win on your own.");

    RoleOptionsGroup ICustomRole.RoleOptionsGroup => new("Neutral Roles", RFSPalette.NeutralGrayColor, 3);

    ModdedRoleTeams ICustomRole.Team => ModdedRoleTeams.Custom;
}