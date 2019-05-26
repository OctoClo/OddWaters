using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUIEvents : MonoBehaviour
{

    [SerializeField]
    TutorialManager tutorialManager;
    
    void OnTooltipHidden()
    {
        tutorialManager.NextStep();
    }
}
