using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using Stargazer.Networking;
using Stargazer.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Stargazer.Components.Minigames;

[RegisterInIl2Cpp]
public class RoleThiefMinigame(IntPtr ptr) : Minigame(ptr)
{
    public Il2CppReferenceField<GameObject> buttonsParent;
    public Il2CppReferenceField<GameObject> content;
    public Il2CppReferenceField<GameObject> roleButtonPrefab;
    public Il2CppReferenceField<Sprite> crewSprite;
    public Il2CppReferenceField<Sprite> impostorSprite;
    public Il2CppReferenceField<Sprite> unknownSprite;
    public TransitionType TransType = TransitionType.SlideBottom;
    public byte playerId;

    public void Open()
    {
        Coroutines.Start(CoAnimateOpen());
        PlayerControl.LocalPlayer.NetTransform.Halt();
        ControllerManager.Instance.OpenOverlayMenu("RoleThiefMinigame", null);
    }
    public void OnFieldChanged(string val)
    {
          for (int i = 0; i <= buttonsParent.Value.transform.childCount - 1; i++)
          {
              buttonsParent.Value.transform.GetChild(i).gameObject.Destroy();
          }

          int i2 = 0;
          foreach (var r in RoleManager.Instance.AllRoles.ToArray().Where(x => !x.IsDead && x.NiceName.Contains(val)))
          {
              if (i2 == 8) break;
              i2++;
              var btnObj = Instantiate(roleButtonPrefab.Value, buttonsParent.Value.transform);
              var btn = btnObj.GetComponent<Button>();
              btn.onClick.AddListener(new Action(() => ClickButton(r.Role)));
              
              var img = btn.targetGraphic.TryCast<Image>();
              if (r.RoleIconSolid)
              {
                  img?.sprite = r.RoleIconSolid;
              }
              else
                  switch (r.TeamType)
                  {
                      case RoleTeamTypes.Impostor:
                          img?.sprite = impostorSprite;
                          break;
                      case RoleTeamTypes.Crewmate:
                          img?.sprite = crewSprite;
                          break;
                      default:
                          img?.sprite = unknownSprite;
                          break;
                  }
          }
    }
    public void Close()
    {
        gameObject.Destroy();
        ControllerManager.Instance.CloseOverlayMenu("RoleThiefMinigame");
    }
    public IEnumerator CoAnimateOpen()
    {
        Minigame minigame = this;
        minigame.amOpening = true;
        if (minigame.OpenSound && Constants.ShouldPlaySfx())
            SoundManager.Instance.PlaySound(minigame.OpenSound, false);
        float depth = minigame.transform.localPosition.z;
        SpriteRenderer[] rends;
        float timer;
        switch (minigame.TransType)
        {
            case TransitionType.SlideBottom:
                for (timer = 0.0f; timer < 0.25; timer += Time.deltaTime)
                {
                    float t = timer / 0.25f;
                    content.Value.transform.localPosition = new Vector3(minigame.TargetPosition.x, Mathf.SmoothStep(Screen.height * -0.65f, minigame.TargetPosition.y, t), depth);
                    yield return null;
                }
                content.Value.transform.localPosition = new Vector3(minigame.TargetPosition.x, minigame.TargetPosition.y, depth);
                break;
            case TransitionType.Alpha:
                var imgs = minigame.GetComponentsInChildren<Image>();
                for (timer = 0.0f; (double) timer < 0.25; timer += Time.deltaTime)
                {
                    float t = timer / 0.25f;
                    for (int index = 0; index < imgs.Length; ++index)
                        imgs[index].color = Color.Lerp(Palette.ClearWhite, Color.white, t);
                    yield return (object) null;
                }
                for (int index = 0; index < imgs.Length; ++index)
                    imgs[index].color = Color.white;
                break;
            case TransitionType.None:
                minigame.transform.localPosition = new Vector3(minigame.TargetPosition.x, minigame.TargetPosition.y, depth);
                break;
        }
        rends = null;
        if (PlayerControl.LocalPlayer.Data.Role.TeamType == RoleTeamTypes.Crewmate)
            GameManager.Instance.LogicMinigame.OnMinigameOpen();
        minigame.amOpening = false;
    }
    public void ClickButton(RoleTypes role)
    {
        PlayerControl.LocalPlayer.RpcSwapRoles(PlayerControlUtils.GetPlayerById(playerId), (uint)role);
        Close();
    }
    
    public static void CreateAndOpen(byte id)
    {
        var g = UnityObject.Instantiate(Assets.RoleThiefMinigame.LoadAsset(), HudManager.Instance.transform, true);
        g.transform.localPosition = Vector3.zero;
        var s = g.GetComponent<RoleThiefMinigame>();
        Instance = s;
        s.Open();
        s.playerId = id;
    }
}