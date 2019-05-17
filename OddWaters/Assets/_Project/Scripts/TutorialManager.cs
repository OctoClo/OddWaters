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
    [SerializeField]
    float telescopeDragWait = 2;

    [SerializeField]
    InputManager inputManager;
    [SerializeField]
    NavigationManager navigationManager;

    [SerializeField]
    GameObject maskingCube;
    [SerializeField]
    GameObject upMaskingCube;
    [SerializeField]
    GameObject deskMaskingCube;
    [SerializeField]
    Roll rollingDesk;

    void Start()
    {
        StartCoroutine(UpdateStep());
    }

    IEnumerator UpdateStep()
    {
        inputManager.tutorialStep = step;

        switch (step)
        {
            case ETutorialStep.STORM:
                Debug.Log(step);
                upMaskingCube.SetActive(true);
                deskMaskingCube.SetActive(true);
                maskingCube.SetActive(true);
                rollingDesk.enabled = false;
                inputManager.tutorial = true;
                yield return new WaitForSeconds(stormDuration);
                NextStep();
                break;

            case ETutorialStep.TELESCOPE_MOVE:
                Debug.Log(step);
                AkSoundEngine.SetState("SeaIntensity", "CalmSea");
                AkSoundEngine.SetState("Weather", "Fine");
                AkSoundEngine.PostEvent("Play_AMB_Sea", gameObject);
                upMaskingCube.SetActive(false);
                maskingCube.SetActive(false);
                navigationManager.InitializeTelescopeElements();
                break;

            case ETutorialStep.BOAT_MOVE:
                yield return new WaitForSeconds(telescopeDragWait);
                Debug.Log(step);
                upMaskingCube.SetActive(true);
                deskMaskingCube.SetActive(false);
                rollingDesk.enabled = true;
                break;

            case ETutorialStep.TELESCOPE_ZOOM:
                Debug.Log(step);
                upMaskingCube.SetActive(false);
                deskMaskingCube.SetActive(true);
                rollingDesk.enabled = false;
                break;

            case ETutorialStep.GO_TO_ISLAND:
                Debug.Log(step);
                upMaskingCube.SetActive(true);
                deskMaskingCube.SetActive(false);
                rollingDesk.enabled = true;
                break;

            case ETutorialStep.OBJECT_ZOOM:
                Debug.Log(step);
                break;

            case ETutorialStep.OBJECT_ROTATE:
                Debug.Log(step);
                break;

            case ETutorialStep.OBJECT_MOVE:
                Debug.Log(step);
                break;

            case ETutorialStep.NO_TUTORIAL:
                Debug.Log(step);
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
