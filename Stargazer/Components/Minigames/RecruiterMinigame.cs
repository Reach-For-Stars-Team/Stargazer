using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Stargazer.Networking;
using Stargazer.Roles.Impostors.Mastermind;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RegisterInIl2Cpp]
public class RecruiterMinigame(IntPtr ptr) : Minigame(ptr)
{
    public Il2CppReferenceField<GameObject> animParent;
    public Il2CppReferenceField<TextMeshProUGUI> animText;
    public Il2CppReferenceField<TextMeshProUGUI> labelTextRecruiter;
    public Il2CppReferenceField<TextMeshProUGUI> timerText;
    public Il2CppReferenceField<Image> imageRecruiter;
    public Il2CppReferenceField<TextMeshProUGUI> labelTextToBeRecruited;
    public Il2CppReferenceField<Image> imageToBeRecruited;
    public Il2CppReferenceField<Sprite> recruiterRevealedImage;
    public List<Button> buttons;
    public RecruitingInteraction interaction;
    public void Open()
    {
        GetComponent<Canvas>().scaleFactor = 2;
        labelTextRecruiter.Value.text = $"{(interaction.mastermind.AmOwner ? "YOU" : "Mastermind")} <b>(Choosing)</b>";
        labelTextToBeRecruited.Value.text = $"{(interaction.toBeRecruited.AmOwner ? "YOU" : "Target")} <b>(Choosing)</b>";
        imageToBeRecruited.Value.material = new(HatManager.Instance.PlayerMaterial);
        PlayerMaterial.SetColors(interaction.toBeRecruited.cosmetics.ColorId, imageToBeRecruited.Value.material);
        
        imageRecruiter.Value.material = new(HatManager.Instance.PlayerMaterial);
        PlayerMaterial.SetColors(interaction.mastermind.AmOwner ? interaction.mastermind.cosmetics.ColorId : 6, imageRecruiter.Value.material);
        
        buttons = GetComponentsInChildren<Button>().ToList();
        foreach (var button in buttons)
        {
            button.gameObject.SetActive(false);
            button.targetGraphic.material = new(HatManager.Instance.PlayerMaterial);
            PlayerMaterial.SetColors(PlayerControl.LocalPlayer.Data.Color, button.targetGraphic.material);
        }
        Coroutines.Start(Animate());
    }

    public IEnumerator Animate(string text = "Rock Paper Scissors!")
    {
        animParent.Value.SetActive(true);
        animText.Value.text = text;
        animText.Value.gameObject.SetActive(false);
        for (float t = 0; t < 0.40f; t += Time.deltaTime)
        {
            animParent.Value.transform.localScale = new(1, Mathf.Lerp(0, 1, t / 0.3f), 1);
            yield return null;
        }
        animParent.Value.transform.localScale = Vector3.one;
        animText.Value.gameObject.SetActive(true);
        yield return PlayerControl.LocalPlayer.StartCoroutine(Effects.Slide2D(animText.Value.transform, new Vector2(
            1500, 0), new(50, 0), 0.6f));
        yield return PlayerControl.LocalPlayer.StartCoroutine(Effects.Slide2D(animText.Value.transform, new Vector2(50, 0), new(-50, 0), 0.4f));
        yield return PlayerControl.LocalPlayer.StartCoroutine(Effects.Slide2D(animText.Value.transform, new Vector2(-50, 0), new(-1500, 0), 0.6f));
        animParent.Value.gameObject.SetActive(false);
        
        yield return new WaitForSeconds(0.3f);
        foreach (var button in buttons)
        {
            button.gameObject.SetActive(true);
        }
    }
    public void OnButtonClick(string name)
    {
        if (!Enum.TryParse(name, out ChoiceTypes choice)) return;
        PlayerControl.LocalPlayer.RpcUpdateRecruitingInteraction(interaction.id, (uint) choice);

        foreach (var btn in buttons)
        {
            btn.gameObject.SetActive(false);
        }
    }

    public static void CreateAndShow(RecruitingInteraction interaction)
    {
        var menu = UnityObject
            .Instantiate(Stargazer.Assets.RecruiterMinigame.LoadAsset(), HudManager.Instance.transform)
            .GetComponent<RecruiterMinigame>();
        Instance = menu;
        menu.interaction = interaction;
        menu.Open();
    }
}