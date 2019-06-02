using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cutscenes : MonoBehaviour
{
    [SerializeField]
    GameManager gameManager;

    void IntroEnded()
    {
        gameManager.IntroEnded();
    }

    public void OutroEnded()
    {
        SceneManager.LoadScene("StartupMenu");
    }
}
