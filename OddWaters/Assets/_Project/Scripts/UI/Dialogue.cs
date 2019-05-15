using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Dialogue
{
    public DialogueLanguage[] languages;
}

[Serializable]
public class DialogueLanguage
{
    public string name;
    public string[] lines;
}
