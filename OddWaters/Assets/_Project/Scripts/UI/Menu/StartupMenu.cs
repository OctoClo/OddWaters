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
        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
        AkSoundEngine.PostEvent("Play_Menu", gameObject);
        MenusAnimator.SetTrigger("ShowSplashscreen");
    }

    public void MouseEnters()
    {
        AkSoundEngine.PostEvent("Play_Dots", gameObject);
        CursorManager.Instance.SetCursor(ECursor.HOVER);
    }

    public void MouseExits()
    {
        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
    }

    public void OnClickPlay()
    {
        MenusAnimator.SetTrigger("ToGame");
        AkSoundEngine.PostEvent("Play_Start", gameObject);
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
        AkSoundEngine.PostEvent("Play_TelescopeOpen_UI", gameObject);
    }

    public void OnClickCredits()
    {
        MenusAnimator.SetBool("CreditsVisible", true);
        AkSoundEngine.PostEvent("Play_TelescopeOpen_UI", gameObject);
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
        AkSoundEngine.PostEvent("Play_TelescopeOpen_UI", gameObject);
    }

    public void OnClickControlsBack()
    {
        Controls.SetActive(false);
        AkSoundEngine.PostEvent("Play_TelescopeClose_UI", gameObject);
    }

    public void OnClickOptionsBack()
    {
        MenusAnimator.SetBool("OptionsVisible", false);
        AkSoundEngine.PostEvent("Play_TelescopeClose_UI", gameObject);
    }
    public void OnClickCreditsBack()
    {
        MenusAnimator.SetBool("CreditsVisible", false);
        AkSoundEngine.PostEvent("Play_TelescopeClose_UI", gameObject);
    }
}
