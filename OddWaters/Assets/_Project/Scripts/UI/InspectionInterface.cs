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
    TextMeshProUGUI transcriptField;
    string[] transcriptTexts;

    [SerializeField]
    Button[] rotateButtons;

    bool[] axisActive = new bool[3];

    public void InitializeInterface(Transcript transcriptRecto, Transcript transcriptVerso, int side)
    {
        transcriptTexts = new string[2];

        if (transcriptRecto != null)
            transcriptTexts[0] = transcriptRecto.languages[0];

        if (transcriptVerso != null)
            transcriptTexts[1] = transcriptVerso.languages[0];

        DisplayTranscriptSide(side);

        for (int i = 0; i < 3; i++)
            axisActive[i] = true;

        if (transcriptZone.activeSelf)
            ToggleTranscript();
    }

    public void DeactivateAxis(int axis)
    {
        axisActive[axis] = false;
    }

    public void InitializeButtons()
    {
        for (int i = 0; i < 3; i++)
        {
            rotateButtons[(i * 2)].gameObject.SetActive(axisActive[i]);
            rotateButtons[(i * 2)].interactable = (axisActive[i]);
            rotateButtons[(i * 2) + 1].gameObject.SetActive(axisActive[i]);
            rotateButtons[(i * 2) + 1].interactable = (axisActive[i]);
        }
    }

    public void SetButtonsInteractable(bool active)
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
        SetButtonsInteractable(!transcriptActive);
        transcriptZone.SetActive(transcriptActive);
    }

    public void DisplayTranscriptSide(int side)
    {
        string newTranscript = transcriptTexts[side];
        if (newTranscript != null)
        {
            transcriptField.text = newTranscript;
            transcriptButton.gameObject.SetActive(true);
        }
        else
            transcriptButton.gameObject.SetActive(false);
    }
}
