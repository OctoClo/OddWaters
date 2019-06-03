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
    Translator translator;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void UpdateTranslator()
    {
        translator = GameObject.FindGameObjectWithTag("Translator").GetComponent<Translator>();
    }

    public void ChangeLanguage(ELanguage newLanguage)
    {
        language = newLanguage;
        translator.UpdateUITexts();
    }
}
