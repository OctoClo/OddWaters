using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField]
    bool splashscreen = false;
    [SerializeField]
    bool startupMenu = false;
    [SerializeField]
    bool intro = false;
    [SerializeField]
    bool tutorial = false;

    [Header("States")]
    [HideInInspector]
    bool paused = false;

    [Header("Managers")]
    [SerializeField]
    TutorialManager tutorialManager;

    [Header("Animators")]
    [SerializeField]
    Animator menusAnimator;
    [SerializeField]
    Animator cutsceneAnimator;
    [SerializeField]
    Animator globalAnimator;
    [SerializeField]
    Animator tutorialUIAnimator;

    void Awake()
    {
        tutorialManager.SetActive(tutorial);
        AkSoundEngine.PostEvent("Play_AMB_Sea", gameObject);

        if (intro)
            PlayIntro();
        else if (tutorial)
            tutorialManager.Launch();
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
        AkSoundEngine.PostEvent("Play_AMB_Sea", gameObject);
        cutsceneAnimator.SetTrigger("Intro");
    }

    public void IntroEnded()
    {
        tutorialManager.Launch();
    }

    void OnGameFinishedEvent(GameFinishedEvent e)
    {
        cutsceneAnimator.SetTrigger("Outro");
    }
}
