using System;
using MiraAPI.Utilities;
using UnityEngine;

namespace Stargazer.Features.MainMenu;

public static class ReworkedMainMenu
{
    public static void SetUp(MainMenuManager menu)
    {
        menu.transform.FindChild("MainUI/AspectScaler/BackgroundTexture").GetComponent<SpriteRenderer>().color = new(0, 0.5f, 0.8f, 1);
        foreach (var btn in menu.GetComponentsInChildren<PassiveButton>())
        {
            btn.inactiveSprites.GetComponent<SpriteRenderer>().material.color = new(0, 0.75f, 1.5f, 1);
            btn.activeSprites.GetComponent<SpriteRenderer>().material.color = new(0, 0.75f, 1.5f, 1);
        }

        menu.mainButtons[0].transform.parent.parent.GetComponent<SpriteRenderer>().material.color = new(0f, 0.9f, 1.1f, 1);
    }
}