using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactible : MonoBehaviour
{
    Rigidbody rigidBody;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    public virtual void Test()
    {

    }

    public void MoveTo(Vector3 newPosition)
    {
        rigidBody.MovePosition(newPosition);
    }
}
