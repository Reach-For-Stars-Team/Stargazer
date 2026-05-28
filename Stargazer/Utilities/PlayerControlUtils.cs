using System;
using System.Collections.Generic;
using System.Linq;
using Cpp2IL.Core.Extensions;
using MiraAPI.Utilities;
using PowerTools;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Utilities;

public static class PlayerControlUtils
{
    public static PlayerControl GetPlayerById(byte id)
    {
        return PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.PlayerId == id);
    }

    public static void Toggle(this CosmeticsLayer c, bool showHat, bool showName, bool showVisor, bool showSkin,
        bool showPet)
    {
        c.ToggleHat(showHat);
        c.ToggleName(showName);
        c.ToggleVisor(showVisor);
        c.ToggleSkin(showSkin);
        c.TogglePet(showPet);
    }

    public static void ToggleSkin(this CosmeticsLayer c, bool show)
    {
        c.skin.gameObject.SetActive(show);
    }

    public static SpriteAnim GetAnimator(this PlayerControl p)
    {
        return p.MyPhysics.Animations.Animator;
    }

    public static void RegenerateTasks(this PlayerControl player)
    {
        NormalPlayerTask[] LongTasks = [];
        ShipStatus.Instance.LongTasks.ToList().CopyTo(LongTasks);

        List<byte> SelectedTasks = new();
        for (var i = 0; i != GameOptionsManager.Instance.CurrentGameOptions.TotalTaskCount; i++)
        {
            var SelectedTask = LongTasks.Random();
            SelectedTasks.Add((byte)SelectedTask.Index);
            var newLongtasks = LongTasks.ToList();
            newLongtasks.Remove(SelectedTask);
            LongTasks = newLongtasks.ToArray();
        }

        player.Data.SetTasks(SelectedTasks.ToArray());
    }
    
    public static GameObject CreateFakePlayer(PlayerControl source)
    {
        var clone = UnityObject.Instantiate(HudManager.Instance.IntroPrefab.PlayerPrefab);
        clone.transform.position = source.transform.position;
        clone.transform.localScale = source.transform.localScale / 2;
        clone.UpdateFromPlayerData(source.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, true, null, true);
        var mask = LayerMask.NameToLayer("Players");
        foreach (var obj in clone.GetComponentsInChildren<GameObject>())
        {
            obj.layer = mask;
        }
        clone.SetName(source.cosmetics.nameText.text);
        clone.SetNamePosition(Vector3.up * 2);
        clone.cosmetics.SetEnabledColorblind(true);
        clone.SetNameScale(source.cosmetics.nameText.transform.localScale * 2);
        return clone.gameObject;
    }
}