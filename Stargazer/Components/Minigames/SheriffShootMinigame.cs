using System;
using System.Collections;
using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Stargazer.Roles.Crewmates.Sheriff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stargazer.Components.Minigames
{
    [RegisterInIl2Cpp]
    public class SheriffShootMinigame(IntPtr ptr) : Minigame(ptr)
    {
        public int availableBullets;
        public int loadedBullets;
        public Il2CppReferenceField<GameObject> bulletLoadPrefab;
        public Il2CppReferenceField<GameObject> Content;
        public Il2CppReferenceField<TextMeshProUGUI> bulletsLeftText;

        public void OnBulletBtnClicked(Button button)
        {
            if (availableBullets == 0) return;
            loadedBullets++;
            availableBullets--;
            button.interactable = false;
            button.gameObject.SetActive(false);
            
            var bulletLoad = Instantiate(bulletLoadPrefab.Value, transform);
            bulletLoad.transform.localPosition = button.transform.localPosition + new Vector3(0, 40, 0);
            StartCoroutine(Effects.ScaleIn(bulletLoad.transform, 1.4f, 1f, 1));
            StartCoroutine(Effects.Rotate2D(bulletLoad.transform, 180, -360, 0.7f));
            bulletsLeftText.Value.text = availableBullets.ToString() + " Bullets";
        }
        
        public void Close()
        {
            Destroy(gameObject);
            ControllerManager.Instance.CloseOverlayMenu("SheriffShootMinigame");
            var r = PlayerControl.LocalPlayer.Data.Role.TryCast<SheriffRole>();
            if (r && loadedBullets > 0)
            {
                r.loadedBullets = loadedBullets;
                r.pulledOutGun = true;
            }
        }
        
        public void Open()
        {
            Coroutines.Start(CoAnimateOpen());
            PlayerControl.LocalPlayer.NetTransform.Halt();
            ControllerManager.Instance.OpenOverlayMenu("SheriffShootMinigame", null);
            bulletsLeftText.Value.text = availableBullets.ToString() + " Bullets";
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
                        Content.Value.transform.localPosition = new Vector3(minigame.TargetPosition.x, Mathf.SmoothStep(Screen.height * -0.65f, minigame.TargetPosition.y, t), depth);
                        yield return null;
                    }
                    Content.Value.transform.localPosition = new Vector3(minigame.TargetPosition.x, minigame.TargetPosition.y, depth);
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
        public static void CreateAndOpen(SheriffRole r)
        {
            var g = UnityObject.Instantiate(Assets.SheriffShootMinigame.LoadAsset(), HudManager.Instance.transform, true);
            g.transform.localPosition = Vector3.zero;
            var s = g.GetComponent<SheriffShootMinigame>();
            Instance = s;
            s.Open();
            s.availableBullets = r.availableBullets;
            s.bulletsLeftText.Value.text = s.availableBullets.ToString() + " Bullets";
        }
    }
}