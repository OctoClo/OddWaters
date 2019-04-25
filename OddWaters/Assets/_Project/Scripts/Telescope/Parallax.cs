using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField]
    float parallaxSpeed = 0.2f;

    GameObject telescopeParent;
    float telescopeParentLastX;

    void Start()
    {
        telescopeParent = transform.parent.parent.gameObject;
        telescopeParentLastX = telescopeParent.transform.position.x;
    }

    void Update()
    {
        float deltaX = telescopeParent.transform.position.x - telescopeParentLastX;
        transform.position += Vector3.right * (deltaX * parallaxSpeed);
        telescopeParentLastX = telescopeParent.transform.position.x;
    }
}
