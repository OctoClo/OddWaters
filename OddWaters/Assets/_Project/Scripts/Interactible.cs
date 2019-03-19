using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactible : MonoBehaviour
{
    Camera mainCamera;
    Vector3 verticalGrabOffset;
    Rigidbody rigidBody;

    void Start()
    {
        mainCamera = Camera.main;
        rigidBody = GetComponent<Rigidbody>();
    }

    public virtual void Test()
    {

    }

    public void Grab()
    {
        rigidBody.useGravity = false;
        Vector3 verticalGrabOffset = mainCamera.transform.position - gameObject.transform.position;
        verticalGrabOffset.Normalize();
        verticalGrabOffset.y *= 2;
        gameObject.transform.position += verticalGrabOffset;
    }

    public void MoveTo(Vector3 newPosition)
    {
        rigidBody.MovePosition(newPosition);
    }

    public void Drop()
    {
        rigidBody.useGravity = true;
    }
}
