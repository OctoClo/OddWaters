using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ELanguage
{
    ENGLISH,
    FRENCH
}

public class LanguageManager : Singleton<LanguageManager>
{
    public ELanguage language = ELanguage.ENGLISH; 
}
