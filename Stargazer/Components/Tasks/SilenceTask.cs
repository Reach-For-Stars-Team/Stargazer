using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Il2CppSystem.Text;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using Stargazer.Roles.Impostors.Silencer;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;

namespace Stargazer.Components.Tasks;
[RegisterInIl2Cpp(typeof(IHudOverrideTask))]
public class SilenceTask(IntPtr ptr) : SabotageTask(ptr)
{
    public override void AppendTaskText(StringBuilder sb)
    {
        sb.AppendLine("<color=red>Shh! Meetings, reports, and noisemakers are temporarily disabled.</color>");
    }

    public override bool IsComplete => false;
    public uint Id = 999;

    private GameObject shushTextContainer;
    public Material grayscaleMaterial;
    public float lifetime;
    private float duration = OptionGroupSingleton<SilencerOptions>.Instance.AbilityDuration.Value;

    private float GetCalculatedStrength()
    {
        float fadeInTime = 2;
        float calculatedStrength;
        if (lifetime <= fadeInTime)
        {
            calculatedStrength = Mathf.Lerp(0, 1, lifetime);
        }
        else if (lifetime >= duration)
        {
            calculatedStrength = Mathf.Lerp(1, 0, lifetime / duration);
        }
        else
        {
            calculatedStrength = 1f;
        }
        return calculatedStrength;
    }
    private void FixedUpdate()
    {
        HudManager.Instance.ReportButton.gameObject.SetActive(false);
        lifetime += Time.deltaTime;
        grayscaleMaterial.SetFloat("_Strength", GetCalculatedStrength());
        if (lifetime >= duration + 1f)
        {
            Remove();
        }
    }

    [RegisterEvent]
    private static void OnMurder(AfterMurderEvent e)
    {
        if (e.DeadBody && SilenceTask.TryGetTaskOfType<SilenceTask>(PlayerControl.LocalPlayer, out SilenceTask task))
        {
            e.DeadBody.myCollider.gameObject.GetComponent<PassiveButton>().enabled = false;
            e.DeadBody.enabled = false;
            task.allDisabledBodies.Add(e.DeadBody);
        }
    }

    public List<DeadBody> allDisabledBodies = new();

    public void Remove()
    {
        SoundManager.Instance.gameObject.SetActive(true);
        foreach (var b in allDisabledBodies)
        {
            b.myCollider.GetComponent<PassiveButton>().enabled = true;
            b.enabled = true;
        }
        CameraFXComponent.Instance.materials.Remove(grayscaleMaterial);
        grayscaleMaterial.Destroy();
        gameObject.Destroy();
        PlayerControl.LocalPlayer.myTasks.Remove(this);
        HudManager.Instance.ReportButton.gameObject.SetActive(true);
    }

    private void Start()
    {
        SoundManager.Instance.gameObject.SetActive(false);
        foreach (var b in UnityObject.FindObjectsOfType<DeadBody>(true))
        {
            b.myCollider.gameObject.GetComponent<PassiveButton>().enabled = false;
            b.enabled = false;
            allDisabledBodies.Add(b);
        }
        HudManager.Instance.ReportButton.gameObject.SetActive(false);
        grayscaleMaterial = new(Assets.GrayscaleMaterial.LoadAsset());
        CameraFXComponent.Instance?.materials.Add(grayscaleMaterial);
        shushTextContainer = new GameObject("shushText");
        shushTextContainer.transform.parent = HudManager.Instance.transform;
        shushTextContainer.transform.localPosition = Vector3.zero;
        
        var letters = new List<TextMeshPro>();
        int xPos = -3;
        foreach (char c in "Shush!")
        {
            var tmp = UnityObject.Instantiate(HudManager.Instance.KillButton.buttonLabelText, shushTextContainer.transform);
            tmp.GetComponent<TextTranslatorTMP>().Destroy();
            tmp.text = c.ToString();
            tmp.transform.localPosition = new Vector3(xPos, 0, -10);
            tmp.transform.localScale = Vector3.zero;
            tmp.fontSize = 64;
            tmp.fontStyle = FontStyles.Italic;
            xPos++;
            tmp.color = Color.gray;
            letters.Add(tmp);
        }

        Coroutines.Start(CoAnimateShushText(letters));
    }

    private IEnumerator CoAnimateShushText(List<TextMeshPro> letters)
    {
        var wait = new WaitForSeconds(0.2f);
        foreach (TextMeshPro tmp in letters)
        {
            Coroutines.Start(CoAnimateShushLetter(tmp, 0.7f));
            yield return wait;
        }
        yield return new WaitForSeconds(1f);
        shushTextContainer.gameObject.Destroy();
        yield break;
    }

    private IEnumerator CoAnimateShushLetter(TextMeshPro tmp, float duration)
    {
        yield return tmp.StartCoroutine(Effects.Bloop(0, tmp.transform, 3, duration*0.3f));
        yield return new WaitForSeconds(duration*0.1f);
        yield return tmp.StartCoroutine(Effects.Slide2D(tmp.transform, tmp.transform.localPosition, new(tmp.transform.localPosition.x, 10), duration*0.6f));
        tmp.gameObject.Destroy();
        yield break;
    }
}