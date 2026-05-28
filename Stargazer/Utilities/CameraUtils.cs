using System.Collections;
using Reactor.Utilities;
using UnityEngine;

namespace Stargazer.Utilities;

public class CameraUtils
{
    public static void ZoomInOut(float target, float duration)
    {
        Coroutines.Start(CoZoomInOut(target, duration));
    }

    private static IEnumerator CoZoomInOut(float target, float duration)
    {
        float prev = Camera.main.orthographicSize;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            Camera.main.gameObject.layer = LayerMask.NameToLayer("Default");
            Camera.main.orthographicSize = Mathf.SmoothStep(prev, target,  t / duration);
            HudManager.Instance.UICamera.orthographicSize = Mathf.SmoothStep(prev, target,  t / duration);
            ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
            yield return null;
        }

        Camera.main.orthographicSize = target;
        HudManager.Instance.UICamera.orthographicSize = target;
        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
        yield break;
    }
}