using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    ETutorialStep step = ETutorialStep.STORM;

    [SerializeField]
    float stormDuration = 2;
    public float telescopeDragWait = 2;

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

    void Start()
    {
        StartCoroutine(UpdateStep());
    }

    IEnumerator UpdateStep()
    {
        Debug.Log(step);
        inputManager.tutorialStep = step;

        switch (step)
        {
            case ETutorialStep.STORM:
                upMaskingCube.SetActive(true);
                deskMaskingCube.SetActive(true);
                maskingCube.SetActive(true);
                rollingDesk.enabled = false;
                inputManager.tutorial = true;
                yield return new WaitForSeconds(stormDuration);
                NextStep();
                break;

            case ETutorialStep.TELESCOPE_MOVE:
                AkSoundEngine.SetState("SeaIntensity", "CalmSea");
                AkSoundEngine.SetState("Weather", "Fine");
                AkSoundEngine.PostEvent("Play_AMB_Sea", gameObject);
                upMaskingCube.SetActive(false);
                maskingCube.SetActive(false);
                navigationManager.InitializeTelescopeElements();
                break;

            case ETutorialStep.BOAT_MOVE:
                telescope.SetImageAlpha(true);
                deskMaskingCube.SetActive(false);
                rollingDesk.enabled = true;
                navigationManager.goalCollider = boatGoalCollider;
                break;

            case ETutorialStep.TELESCOPE_ZOOM:
                boatGoalCollider.gameObject.SetActive(false);
                telescope.SetImageAlpha(false);
                panelUI.SetActive(true);
                telescope.tutorial = true;
                rollingDesk.enabled = false;
                break;

            case ETutorialStep.GO_TO_ISLAND:
                telescope.SetImageAlpha(true);
                panelUI.SetActive(false);
                telescope.tutorial = false;
                rollingDesk.enabled = true;
                navigationManager.goalCollider = firstIslandCollider;
                break;

            case ETutorialStep.OBJECT_ZOOM:
                break;

            case ETutorialStep.OBJECT_ROTATE:
                break;

            case ETutorialStep.OBJECT_MOVE:
                break;

            case ETutorialStep.NO_TUTORIAL:
                upMaskingCube.SetActive(false);
                inputManager.tutorial = false;
                break;
        }
    }

    public void NextStep()
    {
        step++;
        StartCoroutine(UpdateStep());
    }
}
