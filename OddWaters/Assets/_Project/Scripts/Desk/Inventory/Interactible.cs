using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ERotation { R0, R90, R180 };

public class Interactible : MonoBehaviour
{
    [Tooltip("Ordre X - Y - Z")]
    public ERotation[] rotationsAmount = new ERotation[3];

    Camera mainCamera;
    Vector3 verticalGrabOffset;
    Rigidbody rigidBody;

    void Start()
    {
        mainCamera = Camera.main;
        rigidBody = GetComponent<Rigidbody>();
    }

    public virtual void Trigger()
    {

    }

    public void Grab()
    {
        rigidBody.useGravity = false;
        Vector3 verticalGrabOffset = mainCamera.transform.position - gameObject.transform.position;
        verticalGrabOffset.Normalize();
        if (rigidBody.velocity == Vector3.zero)
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

    public void Rotate(int axis, int direction)
    {
        Vector3 rotation = Vector3.zero;

        if (axis == 0)
            rotation.x = getRotation(axis);
        else if (axis == 1)
            rotation.z = getRotation(axis);
        else if (axis == 2)
            rotation.y = getRotation(axis);

        rotation *= direction;
        transform.rotation *= Quaternion.Euler(rotation);
    }

    int getRotation(int axis)
    {
        if (rotationsAmount[axis] == ERotation.R90)
            return 90;
        if (rotationsAmount[axis] == ERotation.R180)
            return 180;
        return 0;
    }
}
