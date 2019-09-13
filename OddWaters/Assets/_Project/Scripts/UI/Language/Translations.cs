using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Translation
{
    public TranslationLanguage[] languages;
}

[Serializable]
public class TranslationLanguage
{
    public string[] texts;
}