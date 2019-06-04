using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField]
    bool intro = false;
    [SerializeField]
    bool tutorial = false;

    [Header("References")]
    [SerializeField]
    TutorialManager tutorialManager;
    [SerializeField]
    Animator cutsceneAnimator;
    [SerializeField]
    GameObject pauseButton;

    void Awake()
    {
        tutorialManager.SetActive(tutorial);
        AkSoundEngine.PostEvent("Play_AMB_Sea", gameObject);

        if (intro)
            PlayIntro();
        else if (tutorial)
        {
            pauseButton.SetActive(true);
            tutorialManager.Launch();
        }
    }

    void OnEnable()
    {
        EventManager.Instance.AddListener<GameFinishedEvent>(OnGameFinishedEvent);
    }

    void OnDisable()
    {
        EventManager.Instance.RemoveListener<GameFinishedEvent>(OnGameFinishedEvent);
    }

    void PlayIntro()
    {
        AkSoundEngine.PostEvent("Play_Intro", gameObject);
        cutsceneAnimator.gameObject.SetActive(true);
        cutsceneAnimator.SetTrigger("Intro");
    }

    public void IntroEnded()
    {
        pauseButton.SetActive(true);
        tutorialManager.Launch();
    }

    void OnGameFinishedEvent(GameFinishedEvent e)
    {
        pauseButton.SetActive(false);
        AkSoundEngine.PostEvent("Stop_All", gameObject);
        cutsceneAnimator.gameObject.SetActive(true);
        cutsceneAnimator.Play("Default");
        cutsceneAnimator.SetTrigger("Outro");
    }
}
