using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum ETutorialStep
{
    STORM,
    TELESCOPE_MOVE,
    BOAT_MOVE,
    TELESCOPE_ZOOM,
    GO_TO_ISLAND,
    OBJECT_ZOOM,
    OBJECT_ROTATE,
    WAITING,
    OBJECT_MOVE,
    NO_TUTORIAL
}

public class TutorialManager : MonoBehaviour
{
    [SerializeField]
    bool tutorial = true;
    [HideInInspector]
    public ETutorialStep step = ETutorialStep.STORM;

    [SerializeField]
    float stormDuration = 2;
    public float telescopeDragWait = 2;
    [SerializeField]
    float lastSentenceDuration = 4;

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
    GameObject maskingCube;
    [SerializeField]
    GameObject upMaskingCube;
    [SerializeField]
    Telescope telescope;
    [SerializeField]
    GameObject deskMaskingCube;
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

    void Start()
    {
        //inputManager.tutorial = tutorial;

        /*if (!tutorial)
        {
            step = ETutorialStep.NO_TUTORIAL;
            LaunchAmbiance();
            StartCoroutine(navigationManager.InitializeTelescopeElements());
        }
        else
        {
            tutorialText = JsonUtility.FromJson<TutorialText>(tutorialJSON.text);
            StartCoroutine(UpdateStep());
        }*/

        if (!tutorial)
        {
            step = ETutorialStep.NO_TUTORIAL;
            //LaunchAmbiance();
            StartCoroutine(navigationManager.InitializeTelescopeElements());
        }
    }

    public void SetActive(bool state)
    {
        tutorial = state;
        inputManager.tutorial = tutorial;

        if (tutorial) Setup();
    }

    public void Setup()
    {

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
            case ETutorialStep.STORM:
                //LaunchAmbiance();

                //deskMaskingCube.SetActive(true);
                //maskingCube.SetActive(true);
                //rollingDesk.enabled = false;
                yield return new WaitForSeconds(stormDuration);
                NextStep();
                break;

            case ETutorialStep.TELESCOPE_MOVE:
                yield return StartCoroutine(navigationManager.InitializeTelescopeElements());

                globalAnimator.SetTrigger("Reveal Telescope");

                PromptTooltip();

                //maskingCube.SetActive(false);
                //tutorialField.transform.parent.gameObject.SetActive(true);
                break;

            case ETutorialStep.BOAT_MOVE:
                globalAnimator.SetTrigger("Reveal Desk");
                telescope.SetImageAlpha(true);
                deskMaskingCube.SetActive(false);
                navigationManager.goalCollider = boatGoalCollider;

                PromptTooltip();

                break;

            case ETutorialStep.TELESCOPE_ZOOM:
                boatGoalCollider.gameObject.SetActive(false);
                telescope.SetImageAlpha(false);
                panelUI.SetActive(true);
                telescope.tutorial = true;
                rollingDesk.enabled = false;

                PromptTooltip();

                break;

            case ETutorialStep.GO_TO_ISLAND:
                telescope.SetImageAlpha(true);
                panelUI.SetActive(false);
                telescope.tutorial = false;
                rollingDesk.enabled = true;
                navigationManager.goalCollider = firstIslandCollider;

                PromptTooltip();

                break;

            case ETutorialStep.OBJECT_ZOOM:
                PromptTooltip();

                break;

            case ETutorialStep.OBJECT_ROTATE:
                PromptTooltip();

                break;

            /*case ETutorialStep.WAITING:
                yield return new WaitForSeconds(2);
                //tutorialField.transform.parent.gameObject.SetActive(false);
                CompleteStep();
                break;*/

            case ETutorialStep.OBJECT_MOVE:

                PromptTooltip();

                break;

            case ETutorialStep.NO_TUTORIAL:
                //upMaskingCube.SetActive(false);
                inputManager.tutorial = false;

                PromptTooltip();

                yield return new WaitForSeconds(lastSentenceDuration);

                CompleteStep();

                break;
        }
    }

    void PromptTooltip()
    {
        UpdateTutorialText();
        tutorialUIAnimator.SetBool("StepCompleted", false);
    }

    public void CompleteStep()
    {
        tutorialUIAnimator.SetBool("StepCompleted", true);
    }

    public void NextStep()
    {
        step++;
        StartCoroutine(UpdateStep());

    }

    void UpdateTutorialText()
    {
        tutorialField.text = tutorialText.languages[(int)LanguageManager.Instance.language].steps[(int)step - 1];
    }

    void LaunchAmbiance()
    {
        AkSoundEngine.SetState("SeaIntensity", "CalmSea");
        AkSoundEngine.SetState("Weather", "Fine");
        AkSoundEngine.PostEvent("Play_AMB_Sea", gameObject);
    }

    public void MaskUp()
    {

    }

    public void MaskBottom()
    {

    }
}
