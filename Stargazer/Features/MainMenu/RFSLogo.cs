using Stargazer.Utilities;
using UnityEngine;

namespace Stargazer.Features.MainMenu;

public static class RFSLogo
{
    public static void Create(MainMenuManager __instance)
    {
        GameObject aulogo = GameObject.Find("LOGO-AU");
        Transform parent = aulogo.transform.parent;
        aulogo.SetActive(false);
        GameObject rfslogo = UnityObject.Instantiate(Assets.RfsLogo.LoadAsset(), parent);
        rfslogo.transform.localPosition = Vector3.zero;
        PassiveButtonUtils.CreatePassiveButton(rfslogo, rfslogo.transform.GetChild(1).gameObject, rfslogo.transform.GetChild(0).gameObject, Credits.CreditsBtnClickListener(__instance));
    }
}