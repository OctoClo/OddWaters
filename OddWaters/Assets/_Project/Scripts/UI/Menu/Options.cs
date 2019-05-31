using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Options : MonoBehaviour
{

    [SerializeField]
    GameObject Controls;

    [SerializeField]
    Animator ENAnimator;
    [SerializeField]
    Animator FRAnimator;

    private void Start()
    {

        //Get current language, select button
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
        SetLanguage(language);
    }

    void SetLanguage(string language)
    {

    }
}
