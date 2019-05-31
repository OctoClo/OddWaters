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

    // Start is called before the first frame update
    void Start()
    {
        if (active)
        {
            Launch();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Launch()
    {
        MenusAnimator.SetTrigger("ShowSplashscreen");
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
        Application.Quit();
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
