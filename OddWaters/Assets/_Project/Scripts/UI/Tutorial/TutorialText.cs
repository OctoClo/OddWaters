using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TutorialText
{
    public TutorialTextLine[] languages;
}

[Serializable]
public class TutorialTextLine
{
    public string[] steps;
}