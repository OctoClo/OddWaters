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

    private void OnEnable()
    {
        ENAnimator.SetTrigger("Activate");
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
        LanguageManager.Instance.language = language.Equals("EN") ? ELanguage.ENGLISH : ELanguage.FRENCH;
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
