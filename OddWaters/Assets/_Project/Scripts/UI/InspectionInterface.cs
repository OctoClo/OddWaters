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
    Transcript[] transcripts;

    [SerializeField]
    Button[] rotateButtons;

    bool[] axisActive = new bool[3];

    public void InitializeInterface(Transcript transcriptRecto, Transcript transcriptVerso, int side)
    {
        transcripts = new Transcript[2];

        if (transcriptRecto != null)
            transcripts[0] = transcriptRecto;

        if (transcriptVerso != null)
            transcripts[1] = transcriptVerso;

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
        Transcript newTranscript = transcripts[side];
        if (newTranscript != null)
        {
            transcriptField.text = "";
            int nbLines = newTranscript.languages[(int)LanguageManager.Instance.language].lines.Length;
            for (int i = 0; i < nbLines; i++)
                transcriptField.text += newTranscript.languages[(int)LanguageManager.Instance.language].lines[i] + "\n";
            transcriptButton.gameObject.SetActive(true);
        }
        else
            transcriptButton.gameObject.SetActive(false);
    }
}
