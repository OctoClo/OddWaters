using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueEvent : GameEvent { public bool ongoing; }

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    ScreenManager screenManager;
    [SerializeField]
    TextMeshProUGUI nameField;
    [SerializeField]
    TextMeshProUGUI textField;

    Queue<string> lines;

    void Start()
    {
        lines = new Queue<string>();    
    }

    public void StartDialogue(Dialogue dialogue)
    {
        EventManager.Instance.Raise(new DialogueEvent() { ongoing = true });

        lines.Clear();
        foreach (string line in dialogue.languages[(int)LanguageManager.Instance.language].lines)
            lines.Enqueue(line);

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
        EventManager.Instance.Raise(new DialogueEvent() { ongoing = false });
    }
}
