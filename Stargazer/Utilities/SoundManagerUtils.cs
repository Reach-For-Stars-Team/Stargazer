using UnityEngine;
using UnityEngine.Audio;

namespace Stargazer.Utilities;

public static class SoundManagerUtils
{
    public static void PlaySoundAtLocation(this SoundManager sound, AudioClip clip, Vector2 location,
        Vector2 hearingLocation, AudioMixerGroup group)
    {
        var v = Vector2.Distance(location, hearingLocation) * -0.2f + 1f;
        if (v < 0f) v = 0f;
        SoundManager.Instance.PlaySound(clip, false, v, group);
    }
}