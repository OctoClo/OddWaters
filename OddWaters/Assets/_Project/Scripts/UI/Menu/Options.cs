using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        currentAnimator = ENAnimator;
        currentAnimator.SetTrigger("Activate");
    }

    private void OnEnable()
    {
        currentAnimator.SetTrigger("Activate");
    }

    public void OnClickControls()
    {
        Controls.SetActive(true);
    }

    public void OnClickControlsBack()
    {
        Controls.SetActive(false);
    }

    public void OnClickLanguageButton(string language)
    {
        if (language.Equals("EN"))
        {
            currentAnimator = ENAnimator;
            LanguageManager.Instance.language = ELanguage.ENGLISH;
        }
        else
        {
            currentAnimator = FRAnimator;
            LanguageManager.Instance.language = ELanguage.FRENCH;
        }
    }

    public void OnVolumeChanged(float value)
    {
        AkSoundEngine.SetRTPCValue("Volume", value * 10);
    }

    public void OnClickQuit()
    {
        AreYouSure.SetActive(true);
    }

    public void OnClickQuitCancel()
    {
        AreYouSure.SetActive(false);
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
