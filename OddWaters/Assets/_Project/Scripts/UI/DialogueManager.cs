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

    Queue<string> lines;
    bool firstEncounter;

    void Start()
    {
        lines = new Queue<string>();    
    }

    public void StartDialogue(Dialogue dialogue, bool first)
    {
        EventManager.Instance.Raise(new DialogueEvent() { ongoing = true, firstEncounter = first });
        firstEncounter = first;

        lines.Clear();

        if (firstEncounter)
        {
            foreach (string line in dialogue.languages[(int)LanguageManager.Instance.language].firstDialogue)
                lines.Enqueue(line);
        }
        else
        {
            foreach (string line in dialogue.languages[(int)LanguageManager.Instance.language].secondDialogue)
                lines.Enqueue(line);
        }

        nameField.text = dialogue.languages[(int)LanguageManager.Instance.language].name;
        NextLine();
    }

    public void NextLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        string line = lines.Dequeue();

        if (line[0] == '*')
        {
            textField.fontStyle = FontStyles.Italic;
            line = line.Substring(1);
        }
        else
            textField.fontStyle = FontStyles.Normal;

        textField.text = line;
        //StopAllCoroutines();
        //StartCoroutine(TypeLine(line));
    }

    IEnumerator TypeLine(string line)
    {
        textField.text = "";
        foreach (char letter in line.ToCharArray())
        {
            textField.text += letter;
            yield return null;
        }
    }

    void EndDialogue()
    {
        EventManager.Instance.Raise(new DialogueEvent() { ongoing = false, firstEncounter = firstEncounter });
    }
}
