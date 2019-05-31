using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupMenu : MonoBehaviour
{
    public bool active = true;

    [SerializeField]
    Animator MenusAnimator;

    [SerializeField]
    GameObject Controls;

    void Start()
    {
        if (active)
        {
            Launch();
        }
    }

    public void Launch()
    {
        AkSoundEngine.PostEvent("Play_AMB_Sea", gameObject);
        AkSoundEngine.SetState("SeaIntensity", "CalmSea");
        AkSoundEngine.SetState("Weather", "Fine");
        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
        MenusAnimator.SetTrigger("ShowSplashscreen");
    }

    public void MouseEnters()
    {
        CursorManager.Instance.SetCursor(ECursor.HOVER);
    }

    public void MouseExits()
    {
        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
    }

    public void OnClickPlay()
    {
        MenusAnimator.SetTrigger("ToGame");
        StartCoroutine(OnPlayAnimEnd());
    }

    IEnumerator OnPlayAnimEnd()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("MainScene");
    }

    public void OnClickOptions()
    {
        MenusAnimator.SetBool("OptionsVisible", true);
    }

    public void OnClickCredits()
    {
        MenusAnimator.SetBool("CreditsVisible", true);
    }

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    public void OnClickOptionsControls()
    {
        Controls.SetActive(true);
    }

    public void OnClickControlsBack()
    {
        Controls.SetActive(false);
    }

    public void OnClickOptionsBack()
    {
        MenusAnimator.SetBool("OptionsVisible", false);
    }
    public void OnClickCreditsBack()
    {
        MenusAnimator.SetBool("CreditsVisible", false);
    }
}
