using System.Collections;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Components;

[RegisterInIl2Cpp]
public class GunpowderSmoke : MonoBehaviour
{
    public bool isLitUp = false;
    public SpriteRenderer renderer;
    public void LightUp()
    {
        if (isLitUp) return;
        isLitUp = true;
        HudManager.Instance.StartCoroutine(Effects.ActionAfterDelay(0.2f, new System.Action(() =>
        {
            foreach (var smoke in Helpers.GetNearestObjectsOfType<GunpowderSmoke>(transform.position, 3, new ContactFilter2D().NoFilter()))
            {
                smoke.LightUp();
            }

            Coroutines.Start(CoExplode());
        })));
    }
    public IEnumerator CoExplode()
    {
        renderer.sprite = Assets.Explosion.LoadAsset();
        HudManager.Instance.StartCoroutine(Effects.ScaleIn(transform, 1, 2, 1));
        HudManager.Instance.StartCoroutine(Effects.ColorFade(renderer, Color.white, Color.clear, 1));
        yield return new WaitForSeconds(1);
        gameObject.Destroy();
    }
}