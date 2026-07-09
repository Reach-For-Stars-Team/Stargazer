using UnityEngine;

namespace Stargazer.Utilities;

public class ParticleUtilities
{
    public static GameObject CreatePhysicsBasedParticle(Vector3 position, Sprite sprite, float gravityScale)
    {
        var go = new GameObject("Smoke");
        go.layer = LayerMask.NameToLayer("Ship");
        go.transform.position = position;
        
        var rend = go.AddComponent<SpriteRenderer>();
        rend.sprite = sprite;

        var circleCollider2D = go.AddComponent<CircleCollider2D>();
        circleCollider2D.radius = 0.2f;
        circleCollider2D.isTrigger = false;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        rb.drag = 0.5f;
        
        rb.includeLayers = LayerMask.GetMask("Ship");
        rb.excludeLayers = LayerMask.GetMask("Players");

        rb.AddRelativeForce(new Vector2(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(0.2f, 2f)
        ),  ForceMode2D.Impulse);

        return go;
    }
}