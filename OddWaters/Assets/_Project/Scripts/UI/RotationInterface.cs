using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotationInterface : MonoBehaviour
{
    [SerializeField]
    Button[] rotateButtons;

    public void DeactivateButtons(int axis)
    {
        rotateButtons[(axis * 2)].interactable = false;
        rotateButtons[(axis * 2) + 1].interactable = false;
    }

    public void ResetButtons()
    {
        for (int i = 0; i < 3; i++)
        {
            rotateButtons[(i * 2)].interactable = true;
            rotateButtons[(i * 2) + 1].interactable = true;
        }
    }
}
