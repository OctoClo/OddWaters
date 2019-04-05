using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotationInterface : MonoBehaviour
{
    [SerializeField]
    Button[] rotateButtons;
    int lastAxisDeactivated;

    public void DeactivateButtons(int axis)
    {
        rotateButtons[(axis * 2)].interactable = false;
        rotateButtons[(axis * 2) + 1].interactable = false;
        lastAxisDeactivated = axis;
    }

    public void ResetButtons()
    {
        rotateButtons[(lastAxisDeactivated * 2)].interactable = true;
        rotateButtons[(lastAxisDeactivated * 2) + 1].interactable = true;
    }
}
