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
        nameField.text = dialogue.languages[0].name;
        lines.Clear();

        foreach (string line in dialogue.languages[0].lines)
            lines.Enqueue(line);

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
        //textField.text = line;
        StopAllCoroutines();
        StartCoroutine(TypeLine(line));
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
        Debug.Log("End of conversation");
        EventManager.Instance.Raise(new DialogueEvent() { ongoing = false });
    }
}
