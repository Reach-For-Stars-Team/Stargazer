using System;
using System.Collections;
using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Stargazer.Roles.Crewmates.Sheriff;
using Stargazer.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stargazer.Components.Minigames
{
    [RegisterInIl2Cpp]
    public class ShotMinigame(IntPtr ptr) : Minigame(ptr)
    {
        public int ButtonsToClick;
        public int ButtonsLeft;
        public float TimeLimit => ButtonsToClick * OptionGroupSingleton<SheriffOptions>.Instance.TimeframePerBullet.Value;
        public bool finished = false;
        public Il2CppReferenceField<Button> buttonPrefab;
        public Il2CppReferenceField<Image> overlaySprite;
        public Il2CppReferenceField<TextMeshProUGUI> titleText;
        public void Open()
        {
            ControllerManager.Instance.OpenOverlayMenu("ShotMinigame", null);
            HudManager.Instance.StartCoroutine(Effects.Shake(titleText.Value.transform, TimeLimit, 5, true, true));
            ButtonsLeft = ButtonsToClick;
            titleText.Value.text = $"Shoot {ButtonsLeft} targets!";
            Vector2 screen = new(Screen.width / 2f, Screen.height / 2f);
            for (int i = 0; i < ButtonsToClick; i++)
            {
                Button button = Instantiate(buttonPrefab.Value, transform);
                button.transform.localPosition = new Vector3(UnityRandom.Range(-screen.x + 35, screen.x - 35), UnityRandom.Range(-screen.y + 35, screen.y - 35), 0);
                button.onClick.AddListener(new SystemAction(() => OnButtonClicked(button)));
            }
            HudManager.Instance.StartCoroutine(Effects.ActionAfterDelay(TimeLimit, new System.Action(() => Close(false))));
        }
        public void OnButtonClicked(Button button)
        {
            button.interactable = false;
            ButtonsLeft--;
            titleText.Value.text = $"Shoot {ButtonsLeft} targets!";
            HudManager.Instance.StartCoroutine(Effects.ActionAfterDelay(0.1f, new SystemAction(() =>
            {
                button.targetGraphic.color = new(1, 1, 1, 0.5f);
                HudManager.Instance.StartCoroutine(Effects.Bloop(0f, button.transform, 1.5f, 0.2f));
                HudManager.Instance.StartCoroutine(Effects.ActionAfterDelay(0.35f, new SystemAction(() => Destroy(button.gameObject))));
            })));
            if (ButtonsLeft <= 0)
            {
                Close(true);
            }
        }
        public void Close(bool success)
        {
            StartCoroutine(Effects.ActionAfterDelay(1f, new SystemAction(() => Close())));
            if (finished) return;
            finished = true;
            if (success)
            {
                titleText.Value.text = "Success!";
                Coroutines.Start(RFSEffects.ColorPulseAndDestroy(overlaySprite.Value, Color.white.ToClearColor(),
                    Color.white, Color.white.ToClearColor(), 0.3f, 0.5f));
            }
            else
            {
                titleText.Value.text = "Failed!";
                Coroutines.Start(RFSEffects.ColorPulseAndDestroy(overlaySprite.Value, Color.red.ToClearColor(),
                    Color.red, Color.red.ToClearColor(), 0.3f, 0.5f));
                PlayerControl.LocalPlayer.RpcCustomMurder(PlayerControl.LocalPlayer, true, false, false, true, false);
            }
        }
        public void Close()
        {
            Destroy(gameObject);
            ControllerManager.Instance.CloseOverlayMenu("ShotMinigame");
        }
        public static void CreateAndOpen(int bulletCount)
        {
            var g = UnityObject.Instantiate(Assets.ShotMinigame.LoadAsset(), HudManager.Instance.transform, true);
            g.transform.localPosition = Vector3.zero;
            var s = g.GetComponent<ShotMinigame>();
            Instance = s;
            s.ButtonsToClick = bulletCount;
            s.Open();
        }
    }
}   