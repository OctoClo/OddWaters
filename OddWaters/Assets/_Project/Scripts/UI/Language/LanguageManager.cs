using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ELanguage
{
    ENGLISH,
    FRENCH
}

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    public ELanguage language = ELanguage.ENGLISH;
    Translator translator;

    void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
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
