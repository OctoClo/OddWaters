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
        inputManager.tutorial = tutorial;

        if (!tutorial)
        {
            step = ETutorialStep.NO_TUTORIAL;
            LaunchAmbiance();
        }
        else
        {
            tutorialText = JsonUtility.FromJson<TutorialText>(tutorialJSON.text);
            StartCoroutine(UpdateStep());
        }        
    }

    IEnumerator UpdateStep()
    {
        Debug.Log(step);

        switch (step)
        {
            case ETutorialStep.STORM:
                upMaskingCube.SetActive(true);
                deskMaskingCube.SetActive(true);
                maskingCube.SetActive(true);
                rollingDesk.enabled = false;
                yield return new WaitForSeconds(stormDuration);
                NextStep();
                break;

            case ETutorialStep.TELESCOPE_MOVE:
                LaunchAmbiance();
                upMaskingCube.SetActive(false);
                maskingCube.SetActive(false);
                tutorialField.transform.parent.gameObject.SetActive(true);
                tutorialField.text = tutorialText.languages[0].steps[(int)step - 1];
                navigationManager.InitializeTelescopeElements();
                break;

            case ETutorialStep.BOAT_MOVE:
                telescope.SetImageAlpha(true);
                deskMaskingCube.SetActive(false);
                rollingDesk.enabled = true;
                navigationManager.goalCollider = boatGoalCollider;
                tutorialField.text = tutorialText.languages[0].steps[(int)step - 1];
                break;

            case ETutorialStep.TELESCOPE_ZOOM:
                boatGoalCollider.gameObject.SetActive(false);
                telescope.SetImageAlpha(false);
                panelUI.SetActive(true);
                telescope.tutorial = true;
                rollingDesk.enabled = false;
                tutorialField.text = tutorialText.languages[0].steps[(int)step - 1];
                break;

            case ETutorialStep.GO_TO_ISLAND:
                telescope.SetImageAlpha(true);
                panelUI.SetActive(false);
                telescope.tutorial = false;
                rollingDesk.enabled = true;
                navigationManager.goalCollider = firstIslandCollider;
                tutorialField.text = tutorialText.languages[0].steps[(int)step - 1];
                break;

            case ETutorialStep.OBJECT_ZOOM:
                tutorialField.text = tutorialText.languages[0].steps[(int)step - 1];
                break;

            case ETutorialStep.OBJECT_ROTATE:
                tutorialField.text = tutorialText.languages[0].steps[(int)step - 1];
                break;

            case ETutorialStep.OBJECT_MOVE:
                tutorialField.text = tutorialText.languages[0].steps[(int)step - 1];
                break;

            case ETutorialStep.NO_TUTORIAL:
                upMaskingCube.SetActive(false);
                inputManager.tutorial = false;
                tutorialField.text = tutorialText.languages[0].steps[(int)step - 1];
                yield return new WaitForSeconds(lastSentenceDuration);
                tutorialField.transform.parent.gameObject.SetActive(false);
                break;
        }
    }

    public void NextStep()
    {
        step++;
        StartCoroutine(UpdateStep());
    }

    void LaunchAmbiance()
    {
        AkSoundEngine.SetState("SeaIntensity", "CalmSea");
        AkSoundEngine.SetState("Weather", "Fine");
        AkSoundEngine.PostEvent("Play_AMB_Sea", gameObject);
    }
}
