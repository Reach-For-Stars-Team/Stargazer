using System.Collections;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stargazer.Utilities;

public class RFSEffects
{
    public static IEnumerator Boop(Transform transform, float yStretch, float xStretch, float duration)
    {
        Vector3 startingScale = transform.localScale;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            Vector3 scale = startingScale;
            scale.x = Mathf.Lerp(xStretch, startingScale.x, t / duration);
            scale.y = Mathf.Lerp(yStretch, startingScale.y, t / duration);
            transform.localScale = scale;
            yield return null;
        }
        transform.localScale = startingScale;
        yield break;
    }

    public static IEnumerator ColorFadeAndDestroy(SpriteRenderer renderer, Color start, Color end, float duration)
    {
        yield return TransitionFade.Instance.StartCoroutine(Effects.ColorFade(renderer, start, end, duration));
        
        renderer.gameObject.Destroy();

        yield break;
    }
    public static IEnumerator ColorFadeAndDestroy(TextMeshPro renderer, Color start, Color end, float duration)
    {
        yield return TransitionFade.Instance.StartCoroutine(Effects.ColorFade(renderer, start, end, duration));
        
        renderer.gameObject.Destroy();

        yield break;
    }
    
    public static IEnumerator MaterialColorFade(Renderer renderer, Color start, Color end, float duration)
    {
        if (!renderer)
        {
            yield break;
        }
        renderer.enabled = true;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            renderer.material.color = Color.Lerp(start, end, t / duration);
            yield return null;
        }
        renderer.material.color = end;
        yield break;
    }
    public static IEnumerator MaterialColorFadeAndDestroy(Renderer renderer, Color start, Color end, float duration)
    {
        yield return Coroutines.Start(MaterialColorFade(renderer, start, end, duration));
        renderer.gameObject.Destroy();
        yield break;
    }
    
    public static IEnumerator ColorPulseAndDestroy(SpriteRenderer renderer, Color start, Color max, Color end, float pulseDuration, float fadeDuration)
    {
        yield return TransitionFade.Instance.StartCoroutine(Effects.ColorFade(renderer, start, max, pulseDuration));
        yield return TransitionFade.Instance.StartCoroutine(Effects.ColorFade(renderer, max, end, fadeDuration)); 
        
        renderer.gameObject.Destroy();

        yield break;
    }

    public static IEnumerator CoMoveArc(Transform target, Vector3 start, Vector3 end, float duration)
    {
        float distance = Vector3.Distance(start, end);
        for (float i = 0; i < duration; i += Time.deltaTime)
        {
            float t = i / duration;
            float x = Mathf.Lerp(start.x, end.x, t);
            float y = Mathf.Lerp(start.y, end.y, t) + Mathf.Sin(t * Mathf.PI) * distance * 0.2f;
            Vector3 newPos = new Vector3(x, y, target.position.z);
            target.position = newPos;
            yield return null;
        }
        target.position = end;
        yield break;
    }
    public static IEnumerator ColorPulseAndDestroy(Graphic renderer, Color start, Color max, Color end, float pulseDuration, float fadeDuration)
    {
        yield return Coroutines.Start(ColorFade(renderer, start, max, pulseDuration));
        yield return Coroutines.Start(ColorFade(renderer, max, end, fadeDuration)); 
        
        renderer.gameObject.Destroy();

        yield break;
    }
    public static IEnumerator ColorFade(
        Graphic self,
        Color source,
        Color target,
        float duration)
    {
        if ((bool) (UnityEngine.Object) self)
        {
            self.enabled = true;
            for (float t = 0.0f; (double) t < (double) duration; t += Time.deltaTime)
            {
                self.color = Color.Lerp(source, target, t / duration);
                yield return (object) null;
            }
            self.color = target;
        }
    }
}