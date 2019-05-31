using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEvents : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnMouseIn()
    {
        animator.SetBool("Hover", true);
    }

    public void OnMouseOut()
    {
        animator.SetBool("Hover", false);
    }

    public void OnClick()
    {
        animator.SetTrigger("Clicked");
    }
}
