using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscenes : MonoBehaviour
{
    [SerializeField]
    GameManager gameManager;

    void IntroEnded()
    {
        gameManager.IntroEnded();
    }

    void OutroEnded()
    {

    }
}
