using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateDialogueUI : MonoBehaviour
{
    [SerializeField]
    Animator dialogueUIAnimator;

    public void Appear()
    {
        dialogueUIAnimator.SetTrigger("Appear");
    }

    public void Disappear()
    {
        dialogueUIAnimator.SetTrigger("Disappear");
    }
}
