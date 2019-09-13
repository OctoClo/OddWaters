using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueEvent : GameEvent { public bool ongoing; public bool firstEncounter; }

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    ScreenManager screenManager;
    [SerializeField]
    TextMeshProUGUI nameField;
    [SerializeField]
    TextMeshProUGUI textField;
    [SerializeField]
    float delayBetweenLetters;

    bool dialogueOngoing;
    Queue<string> lines;
    string currentLine;
    bool typing;
    char[] letters;
    int currentLetterIndex;

    bool firstEncounter;

    void Start()
    {
        dialogueOngoing = false;
        typing = false;
        lines = new Queue<string>();    
    }

    public void StartDialogue(Dialogue dialogue, bool first)
    {
        dialogueOngoing = true;
        firstEncounter = first;

        lines.Clear();

        if (firstEncounter)
        {
            foreach (string line in dialogue.languages[(int)OptionsManager.Instance.language].firstDialogue)
                lines.Enqueue(line);
        }
        else
        {
            foreach (string line in dialogue.languages[(int)OptionsManager.Instance.language].secondDialogue)
                lines.Enqueue(line);
        }

        nameField.text = dialogue.languages[(int)OptionsManager.Instance.language].name;
        NextLine();

        EventManager.Instance.Raise(new DialogueEvent() { ongoing = true, firstEncounter = first });
    }

    public void NextLine()
    {
        if (dialogueOngoing)
        {
            if (typing)
            {
                typing = false;
                StopAllCoroutines();
                textField.text = currentLine;
            }
            else
            {
                if (lines.Count == 0)
                {
                    EndDialogue();
                    return;
                }

                currentLine = lines.Dequeue();

                if (currentLine[0] == '*')
                {
                    textField.fontStyle = FontStyles.Italic;
                    currentLine = currentLine.Substring(1);
                }
                else
                    textField.fontStyle = FontStyles.Normal;

                /*typing = true;
                currentLetterIndex = 0;*/

                letters = currentLine.ToCharArray();
                textField.text = "";

                StopAllCoroutines();
                StartCoroutine(TypeLine());
            }
        }
    }

    /*void Update()
    {
        if (typing)
        {
            textField.text += letters[currentLetterIndex];
            currentLetterIndex++;
            if (currentLetterIndex == currentLine.Length)
                typing = false;
        }
    }*/

    IEnumerator TypeLine()
    {
        typing = true;
        foreach (char letter in letters)
        {
            textField.text += letter;
            yield return new WaitForSeconds(0.016f);
        }
        typing = false;
    }

    void EndDialogue()
    {
        dialogueOngoing = false;
        EventManager.Instance.Raise(new DialogueEvent() { ongoing = false, firstEncounter = firstEncounter });
    }
}
