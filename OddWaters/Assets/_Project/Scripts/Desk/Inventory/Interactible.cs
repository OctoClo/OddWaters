using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ERotation { R0, R90, R180 };

public class Interactible : MonoBehaviour
{
    [SerializeField]
    [Range(1, 4)]
    float rotationSpeed = 3f;
    float rotationSpeedMax = 1.8f;
    [Tooltip("Ordre X - Y - Z")]
    public ERotation[] rotationsAmount = new ERotation[3];
    bool rotating;
    Quaternion rotationBefore;
    Quaternion rotationAfter;
    float rotationTime = 0;
    float currentRotationSpeed;

    Camera mainCamera;
    Rigidbody rigidBody;

    bool zoom;
    Vector3 beforeZoomPosition;
    Vector3 zoomPosition;    

    void Start()
    {
        mainCamera = Camera.main;
        rigidBody = GetComponent<Rigidbody>();
        rotating = false;
        currentRotationSpeed = rotationSpeed;
    }

    public virtual void Trigger()
    {

    }

    public void Grab()
    {
        rigidBody.useGravity = false;
        if (rigidBody.velocity == Vector3.zero)
        {
            Vector3 verticalGrabOffset = mainCamera.transform.position - gameObject.transform.position;
            verticalGrabOffset.Normalize();
            verticalGrabOffset.y *= 2;
            gameObject.transform.position += verticalGrabOffset;
        }
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
        if (rotating)
        {
            currentRotationSpeed = rotationSpeedMax;
            rotationBefore = rotationAfter;
        }
        else
            rotationBefore = transform.rotation;

        rotating = true;
        rotationTime = 0;

        Vector3 axisVec = Vector3.zero;
        if (axis == 0)
            axisVec = Vector3.right;
        else if (axis == 1)
            axisVec = Vector3.up;
        else if (axis == 2)
            axisVec = Vector3.forward;

        rotationAfter = rotationBefore * Quaternion.AngleAxis(getRotation(axis) * direction, axisVec);
    }

    void Update()
    {
        if (rotating)
        {
            rotationTime += Time.deltaTime * currentRotationSpeed;
            transform.rotation = Quaternion.Slerp(rotationBefore, rotationAfter, rotationTime);

            if (zoom)
                gameObject.transform.position = zoomPosition;

            if (rotationTime >= 1)
            {
                rotating = false;
                currentRotationSpeed = rotationSpeed;
            }
        }
    }

    int getRotation(int axis)
    {
        if (rotationsAmount[axis] == ERotation.R90)
            return 90;
        if (rotationsAmount[axis] == ERotation.R180)
            return 180;
        return 0;
    }

    public void EnterRotationInterface()
    {
        zoom = true;
        rigidBody.useGravity = false;
        if (rigidBody.velocity == Vector3.zero)
        {
            beforeZoomPosition = gameObject.transform.position;
            zoomPosition = new Vector3(mainCamera.transform.position.x, beforeZoomPosition.y + 4, 0);
            gameObject.transform.position = zoomPosition;
        }
    }

    public void ExitRotationInterface()
    {
        if (rotating)
        {
            rotating = false;
            currentRotationSpeed = rotationSpeed;
            transform.rotation = rotationAfter;
        }
        gameObject.transform.position = beforeZoomPosition;
        rigidBody.useGravity = true;
        zoom = false;
    }

    public void SetRotationInterfaceAxis(RotationInterface rotationInterface)
    {
        for (int i = 0; i < 3; i++)
        {
            if (rotationsAmount[i] == ERotation.R0)
                rotationInterface.DeactivateButtons(i);
        }
    }
}
