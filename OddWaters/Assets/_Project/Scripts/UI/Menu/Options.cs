using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    GameObject Controls;
    [SerializeField]
    GameObject AreYouSure;

    [SerializeField]
    Animator ENAnimator;
    [SerializeField]
    Animator FRAnimator;
    Animator currentAnimator;

    [SerializeField]
    Slider soundSlider;

    private void Awake()
    {
        if (OptionsManager.Instance.language == ELanguage.ENGLISH)
            currentAnimator = ENAnimator;
        else
            currentAnimator = FRAnimator;
        currentAnimator.SetTrigger("Activate");

        soundSlider.value = OptionsManager.Instance.soundValue;
    }

    private void OnEnable()
    {
        currentAnimator.SetTrigger("Activate");
    }

    public void OnClickControls()
    {
        Controls.SetActive(true);
        AkSoundEngine.PostEvent("Play_TelescopeOpen_UI", gameObject);
    }

    public void OnClickControlsBack()
    {
        Controls.SetActive(false);
        AkSoundEngine.PostEvent("Play_TelescopeClose_UI", gameObject);
    }

    public void OnClickLanguageButton(string language)
    {
        if (language.Equals("EN"))
        {
            currentAnimator = ENAnimator;
            OptionsManager.Instance.ChangeLanguage(ELanguage.ENGLISH);
        }
        else
        {
            currentAnimator = FRAnimator;
            OptionsManager.Instance.ChangeLanguage(ELanguage.FRENCH);
        }
    }

    public void OnVolumeChanged(float value)
    {
        OptionsManager.Instance.soundValue = (int)value;
        AkSoundEngine.SetRTPCValue("Volume", value * 10);
    }

    public void OnClickQuit()
    {
        AreYouSure.SetActive(true);
        AkSoundEngine.PostEvent("Play_TelescopeOpen_UI", gameObject);
    }

    public void OnClickQuitCancel()
    {
        AreYouSure.SetActive(false);
        AkSoundEngine.PostEvent("Play_TelescopeClose_UI", gameObject);
    }

    public void OnClickQuitConfirm()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
