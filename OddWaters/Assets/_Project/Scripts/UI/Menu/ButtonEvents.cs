using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEvents : MonoBehaviour
{

    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
