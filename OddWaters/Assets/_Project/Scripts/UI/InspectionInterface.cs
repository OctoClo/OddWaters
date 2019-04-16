using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InspectionInterface : MonoBehaviour
{
    [SerializeField]
    Button transcriptButton;

    [SerializeField]
    GameObject transcriptZone;

    [SerializeField]
    TextMeshProUGUI transcriptText;
    Transcript currentTranscript;

    [SerializeField]
    Button[] rotateButtons;

    bool[] axisActive = new bool[3];

    public void InitializeInterface(Transcript transcript)
    {
        currentTranscript = transcript;
        if (transcript != null)
        {
            transcriptButton.gameObject.SetActive(true);
            transcriptText.text = currentTranscript.textFrench;
        }
        else
            transcriptButton.gameObject.SetActive(false);

        for (int i = 0; i < 3; i++)
            axisActive[i] = true;

        if (transcriptZone.activeSelf)
            ToggleTranscript();
    }

    public void DeactivateAxis(int axis)
    {
        axisActive[axis] = false;
    }

    public void SetButtonsActive(bool active)
    {
        for (int i = 0; i < 3; i++)
        {
            rotateButtons[(i * 2)].interactable = (active && axisActive[i]);
            rotateButtons[(i * 2) + 1].interactable = (active && axisActive[i]);
        }
    }

    public void ToggleTranscript()
    {
        bool transcriptActive = !transcriptZone.activeSelf;
        SetButtonsActive(!transcriptActive);
        transcriptZone.SetActive(transcriptActive);
    }
}
