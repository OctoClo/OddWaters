using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Translator : MonoBehaviour
{
    [SerializeField]
    TextAsset translationJSON;
    Translation translation;

    public TextMeshProUGUI[] playTexts;
    public TextMeshProUGUI[] optionsTexts;
    public TextMeshProUGUI[] creditsTexts;
    public TextMeshProUGUI[] quitTexts;
    public TextMeshProUGUI[] volumeTexts;
    public TextMeshProUGUI[] languageTexts;
    public TextMeshProUGUI[] controlsTexts;
    public TextMeshProUGUI[] backTexts;
    public TextMeshProUGUI[] VincentTexts;
    public TextMeshProUGUI[] ClaudiaTexts;
    public TextMeshProUGUI[] MeryemTexts;
    public TextMeshProUGUI[] PaulTexts;
    public TextMeshProUGUI[] JeremyTexts;
    public TextMeshProUGUI[] ClementTexts;
    public TextMeshProUGUI[] movePanTexts;
    public TextMeshProUGUI[] movePanControlTexts;
    public TextMeshProUGUI[] leftClickHoldTexts;
    public TextMeshProUGUI[] zoomTexts;
    public TextMeshProUGUI[] zoomControlTexts;
    public TextMeshProUGUI[] talkTexts;
    public TextMeshProUGUI[] talkControlTexts;
    public TextMeshProUGUI[] objectTexts;
    public TextMeshProUGUI[] moveObjectTexts;
    public TextMeshProUGUI[] inspectObjectTexts;
    public TextMeshProUGUI[] inspectObjectControlTexts;
    public TextMeshProUGUI[] closeObjectTexts;
    public TextMeshProUGUI[] closeObjectControlTexts;
    public TextMeshProUGUI[] rotateObjectTexts;
    public TextMeshProUGUI[] rotateObjectControlsTexts;
    public TextMeshProUGUI[] boatTexts;
    public TextMeshProUGUI[] moveBoatTexts;
    public TextMeshProUGUI[] introTexts;
    public TextMeshProUGUI[] thanksTexts;
    public TextMeshProUGUI[] pausedTexts;
    public TextMeshProUGUI[] resumeTexts;
    public TextMeshProUGUI[] quitWarningTexts;
    public TextMeshProUGUI[] quitQuestionTexts;
    public TextMeshProUGUI[] yesTexts;
    public TextMeshProUGUI[] noTexts;

    TextMeshProUGUI[][] texts;

    void Start()
    {
        texts = new TextMeshProUGUI[][]
        {
            playTexts,
            optionsTexts,
            creditsTexts,
            quitTexts,
            volumeTexts,
            languageTexts,
            controlsTexts,
            backTexts,
            VincentTexts,
            ClaudiaTexts,
            MeryemTexts,
            PaulTexts,
            JeremyTexts,
            ClementTexts,
            movePanTexts,
            movePanControlTexts,
            leftClickHoldTexts,
            zoomTexts,
            zoomControlTexts,
            talkTexts,
            talkControlTexts,
            objectTexts,
            moveObjectTexts,
            inspectObjectTexts,
            inspectObjectControlTexts,
            closeObjectTexts,
            closeObjectControlTexts,
            rotateObjectTexts,
            rotateObjectControlsTexts,
            boatTexts,
            moveBoatTexts,
            introTexts,
            thanksTexts,
            pausedTexts,
            resumeTexts,
            quitWarningTexts,
            quitQuestionTexts,
            yesTexts,
            noTexts
        };

        translation = JsonUtility.FromJson<Translation>(translationJSON.text);

        UpdateUITexts();
    }

    public void UpdateUITexts()
    {
        int counter, length;
        for (int i = 0; i < texts.Length; i++)
        {
            length = texts[i].Length;
            for (counter = 0; counter < length; counter++)
                texts[i][counter].text = translation.languages[(int)OptionsManager.Instance.language].texts[i];
        }
    }
}
