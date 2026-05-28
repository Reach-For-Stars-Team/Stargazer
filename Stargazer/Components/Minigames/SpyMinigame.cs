using System;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Map;
using MiraAPI.Patches.Stubs;
using MiraAPI.Utilities;
using Stargazer.Roles.Crewmates.Spy;
using Stargazer.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Stargazer.Components.Minigames;

[RegisterInIl2Cpp]
public class SpyMinigame(IntPtr ptr) : Minigame(ptr)
{
    public SpyRole SpyRole;
    public PassiveButton CloseBtn;
    public TextMeshPro batterytext;
    public void Open()
    {
        if (SpyRole == null) Close();
        batterytext = Helpers.CreateTextLabel("BatteryText", transform.GetChild(0),
            AspectPosition.EdgeAlignments.Center, Vector3.zero, 6f);
        batterytext.GetComponent<AspectPosition>().DestroyImmediate();
        batterytext.color = Color.green;
        batterytext.transform.localPosition = new(0.05f, 0.09f, 0);
        GameObject closeGO = transform.FindChild("CloseBtn").gameObject; 
        CloseBtn = PassiveButtonUtils.CreatePassiveButton(closeGO, closeGO.transform.FindChild("Active").gameObject, closeGO.transform.FindChild("Inactive").gameObject, new(() => Close(true)));
        HudManager.Instance.StartCoroutine(CoAnimateOpen());
        ControllerManager.Instance.OpenOverlayMenu("SpyMinigame", CloseBtn);
        SpyRole.Player.NetTransform.Halt();
    }

    public void FixedUpdate()
    {
        SpyRole.CurrentCharge -= Time.deltaTime;
        batterytext.text = ((int)SpyRole.CurrentCharge).ToString();
        if (SpyRole.CurrentCharge <= 0) Close();
    }

    public void Close()
    {
        gameObject.Destroy();
        ControllerManager.Instance.CloseOverlayMenu("SpyMinigame");
    }
    
    public static void CreateAndOpen()
    {
        var g = UnityObject.Instantiate(Assets.StalkMinigame.LoadAsset());
        g.transform.parent = HudManager.Instance.transform;
        g.transform.localPosition = Vector3.zero;
        var s = g.AddComponent<SpyMinigame>();
        Instance = s;
        s.SpyRole = PlayerControl.LocalPlayer.Data.Role.TryCast<SpyRole>();
        s.Open();
    }
}