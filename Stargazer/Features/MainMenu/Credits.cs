using System;
using Il2CppSystem.Collections.Generic;
using System.Linq;
using Stargazer.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Features.MainMenu;

public static class Credits
{
    public static bool isMenuOpen = false;
    
    public static Action CreditsBtnClickListener(MainMenuManager mainMenu)
    {
        void OnClick()
        {
            if (isMenuOpen) return;
            var go = UnityObject.Instantiate(Assets.CreditsMenuPrefab.LoadAsset());
            go.transform.position = new(0, 0, -10);
            
            List<UiElement> uiElements = new List<UiElement>();
            
            GameObject backBtnGo = go.transform.FindChild("BackBtn").gameObject;
            PassiveButton backBtn = PassiveButtonUtils.CreatePassiveButton(backBtnGo, backBtnGo.transform.FindChild("XMarkSelected").gameObject, backBtnGo.transform.FindChild("XMark").gameObject, ExitBtnClickListener(go));
            uiElements.Add(backBtn);
            
            GameObject discordBtnGo = go.transform.FindChild("DiscordBtn").gameObject;
            PassiveButton discordBtn = PassiveButtonUtils.CreatePassiveButton(discordBtnGo, discordBtnGo.transform.GetChild(0).gameObject, discordBtnGo.transform.GetChild(0).gameObject, LinkBtnClickListener("https://discord.gg/Vg3mR9hRGf"));
            uiElements.Add(discordBtn);
            
            GameObject itchBtnGo = go.transform.FindChild("ItchBtn").gameObject;
            PassiveButton itchBtn = PassiveButtonUtils.CreatePassiveButton(itchBtnGo, itchBtnGo.transform.GetChild(0).gameObject, itchBtnGo.transform.GetChild(0).gameObject, LinkBtnClickListener("https://pixmakesgames.itch.io/reach-for-stars"));
            uiElements.Add(itchBtn);
            
            isMenuOpen = true;
            ControllerManager.Instance.StartCoroutine(Effects.ScaleIn(go.transform, 0, 0.5f, 0.25f));
            ControllerManager.Instance.OpenOverlayMenu("CreditsMenu", backBtn, backBtn, uiElements);
            
            mainMenu.ResetScreen();
        }

        return OnClick;
    }
    private static Action ExitBtnClickListener(GameObject Menu)
    {
        void OnClick()
        {
            Menu.Destroy();
            isMenuOpen = false;
        }

        return OnClick;
    }
    
    private static Action LinkBtnClickListener(string url)
    {
        void OnClick()
        {
            Application.OpenURL(url);
        }

        return OnClick;
    }
}