using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum ETutorialStep
{
    TELESCOPE_MOVE,
    BOAT_MOVE,
    TELESCOPE_ZOOM,
    GO_TO_ISLAND,
    OBJECT_ZOOM,
    OBJECT_ROTATE,
    OBJECT_MOVE,
    NO_TUTORIAL
}

public class TutorialManager : MonoBehaviour
{
    [Header("General")]
    [HideInInspector]
    public ETutorialStep step = ETutorialStep.TELESCOPE_MOVE;
    public float telescopeDragWait = 2;
    [SerializeField]
    float lastSentenceDuration = 4;

    [Header("References")]
    [SerializeField]
    Animator globalAnimator;
    [SerializeField]
    Animator tutorialUIAnimator;
    [SerializeField]
    InputManager inputManager;
    [SerializeField]
    NavigationManager navigationManager;

    [SerializeField]
    GameObject panelUI;
    [SerializeField]
    Telescope telescope;
    [SerializeField]
    Roll rollingDesk;
    [SerializeField]
    Collider boatGoalCollider;
    [SerializeField]
    Collider firstIslandCollider;

    [SerializeField]
    TextMeshProUGUI tutorialField;
    [SerializeField]
    TextAsset tutorialJSON;
    TutorialText tutorialText;

    [HideInInspector]
    public bool stateCompleted = false;
    bool tutorial = true;

    public void SetActive(bool state)
    {
        tutorial = state;
        inputManager.tutorial = tutorial;

        if (tutorial)
            Setup();
        else
        {
            StartCoroutine(navigationManager.InitializeTelescopeElements());
            rollingDesk.enabled = true;
            step = ETutorialStep.NO_TUTORIAL;
        }
    }

    public void Setup()
    {
        rollingDesk.enabled = false;
        globalAnimator.SetTrigger("Setup Tutorial");
    }

    public void Launch()
    {
        tutorialText = JsonUtility.FromJson<TutorialText>(tutorialJSON.text);
        step = ETutorialStep.TELESCOPE_MOVE;
        tutorialUIAnimator.SetTrigger("Start");
        StartCoroutine(UpdateStep());
    }

    IEnumerator UpdateStep()
    {
        Debug.Log(step);

        switch (step)
        {
            case ETutorialStep.TELESCOPE_MOVE:
                yield return StartCoroutine(navigationManager.InitializeTelescopeElements());
                globalAnimator.SetTrigger("Reveal Telescope");
                PromptTooltip();
                break;

            case ETutorialStep.BOAT_MOVE:
                globalAnimator.SetTrigger("Reveal Desk");
                telescope.SetImageAlpha(true);
                navigationManager.goalCollider = boatGoalCollider;
                PromptTooltip();
                break;

            case ETutorialStep.TELESCOPE_ZOOM:
                boatGoalCollider.gameObject.SetActive(false);
                telescope.SetImageAlpha(false);
                panelUI.SetActive(true);
                telescope.tutorial = true;
                PromptTooltip();
                break;

            case ETutorialStep.GO_TO_ISLAND:
                telescope.SetImageAlpha(true);
                panelUI.SetActive(false);
                telescope.tutorial = false;
                navigationManager.goalCollider = firstIslandCollider;
                PromptTooltip();
                break;

            case ETutorialStep.OBJECT_ZOOM:
                PromptTooltip();
                break;

            case ETutorialStep.OBJECT_ROTATE:
                PromptTooltip();
                break;

            case ETutorialStep.OBJECT_MOVE:
                PromptTooltip();
                break;

            case ETutorialStep.NO_TUTORIAL:
                inputManager.tutorial = false;
                rollingDesk.enabled = true;
                PromptTooltip();
                yield return new WaitForSeconds(lastSentenceDuration);
                CompleteStep();
                break;
        }
    }

    void PromptTooltip()
    {
        tutorialField.text = tutorialText.languages[(int)OptionsManager.Instance.language].steps[(int)step];
        tutorialUIAnimator.SetBool("StepCompleted", false);
    }

    public void CompleteStep()
    {
        stateCompleted = true;
        tutorialUIAnimator.SetBool("StepCompleted", true);
    }

    public void NextStep()
    {
        step++;
        StartCoroutine(UpdateStep());
        stateCompleted = false;
    }
}
