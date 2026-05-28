using PowerTools;
using UnityEngine;

namespace Stargazer.Components;

public class SwingingLantern : MonoBehaviour
{
    public SpriteRenderer _renderer;
    public SpriteAnim _animator;
    public PlayerControl Player;

    public void FixedUpdate()
    {
        if (Player.MyPhysics.body.velocity.x != 0) _animator.Play(Assets.LanternBobAnim.LoadAsset());
        _renderer.flipX = Player.MyPhysics.FlipX;
        _renderer.enabled = Player.Visible;
    }
}