using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Transcript
{
    public TranscriptText[] languages;
}

[Serializable]
public class TranscriptText
{
    public string[] lines;
}
