using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEvents : MonoBehaviour
{
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        animator.SetBool("Hover", false);
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
