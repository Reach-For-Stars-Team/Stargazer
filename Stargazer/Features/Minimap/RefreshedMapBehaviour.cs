using System.Collections;
using MiraAPI.LocalSettings;
using Stargazer.Options;
using Reactor.Utilities;
using UnityEngine;

namespace Stargazer.Features.Minimap;

public static class RefreshedMapBehaviour
{
    public static void SetUp(MapBehaviour __instance)
    {
        if (!LocalSettingsTabSingleton<ClientOptions>.Instance.EnableBetterMinimap.Value) return;
        __instance.ColorControl.baseColor = PlayerControl.LocalPlayer.Data.Role.TeamColor;
        Coroutines.Start(Animate(__instance.transform));
    }

    private static IEnumerator Animate(Transform target)
    {
        float duration = 0.2f;
        Vector3 temp = target.localScale * 0.8f;
        temp.x = 1;
        for (float time = 0f; time < duration; time += Time.deltaTime)
        {
            float num = time / duration;
            temp.y = Mathf.SmoothStep(0, 1, num);
            target.transform.localScale = temp;
            yield return null;
        }

        temp.y = 1;
        target.transform.localScale = temp;
        
        yield break;
    }
}