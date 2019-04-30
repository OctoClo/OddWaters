using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ERotation { R0, R90, R180 };

public class Interactible : MonoBehaviour
{
    [SerializeField]
    TextAsset transcriptJSON;
    Transcript transcript;
    [SerializeField]
    [Range(1, 4)]
    float rotationSpeed = 3f;
    [Tooltip("Ordre X - Y - Z")]
    public ERotation[] rotationsAmount = new ERotation[3];

    public InspectionInterface inspectionInterface;

    [HideInInspector]
    public bool rotating;
    Quaternion rotationBefore;
    Quaternion rotationAfter;
    float rotationTime = 0;
    float currentRotationSpeed;

    Camera mainCamera;
    Rigidbody rigidBody;
    BoxCollider boxCollider;

    bool zoom;
    Vector3 beforeZoomPosition;
    Vector3 zoomPosition;
    Transform inventory;

    [SerializeField]
    AK.Wwise.Switch soundMaterial;

    void Start()
    {
        mainCamera = Camera.main;
        rigidBody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        rotating = false;
        currentRotationSpeed = rotationSpeed;
        inventory = transform.parent;
        if (transcriptJSON != null)
            transcript = JsonUtility.FromJson<Transcript>(transcriptJSON.text);
    }

    public virtual void Trigger()
    {

    }

    public bool IsGrabbable()
    {
        return Physics.Raycast(transform.position, -Vector3.up, boxCollider.bounds.extents.y + 0.1f);
    }

    public void Grab()
    {
        AkSoundEngine.PostEvent("Play_Manipulation", gameObject);
        rigidBody.useGravity = false;
        transform.position = GetGrabbedPosition();
    }

    public Vector3 GetGrabbedPosition()
    {
        Vector3 verticalGrabOffset = mainCamera.transform.position - gameObject.transform.position;
        verticalGrabOffset.Normalize();
        verticalGrabOffset.y *= 2;
        return transform.position + verticalGrabOffset;
    }

    public void MoveTo(Vector3 newPosition)
    {
        rigidBody.MovePosition(newPosition);
    }

    public void Drop()
    {
        AkSoundEngine.PostEvent("Play_Manipulation", gameObject);
        rigidBody.useGravity = true;
    }

    public void Rotate(int axis, int direction)
    {
        AkSoundEngine.PostEvent("Play_Manipulation", gameObject);
        inspectionInterface.SetButtonsActive(false);

        rotating = true;
        rotationTime = 0;
        rotationBefore = transform.rotation;

        Vector3 axisVec = Vector3.zero;
        if (axis == 0)
            axisVec = Vector3.right;
        else if (axis == 1)
            axisVec = Vector3.up;
        else if (axis == 2)
            axisVec = Vector3.forward;

        rotationAfter = Quaternion.AngleAxis(getRotation(axis) * direction, axisVec) * rotationBefore;
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
                inspectionInterface.SetButtonsActive(true);
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
        AkSoundEngine.PostEvent("Play_Manipulation", gameObject);
        zoom = true;
        rigidBody.useGravity = false;
        beforeZoomPosition = gameObject.transform.position;
        zoomPosition = new Vector3(mainCamera.transform.position.x, beforeZoomPosition.y + 4, 0);
        gameObject.transform.position = zoomPosition;

        inspectionInterface.InitializeInterface(transcript);
        for (int i = 0; i < 3; i++)
        {
            if (rotationsAmount[i] == ERotation.R0)
                inspectionInterface.DeactivateAxis(i);
        }
        inspectionInterface.SetButtonsActive(true);
        transform.SetParent(null);
    }

    public void ExitRotationInterface()
    {
        AkSoundEngine.PostEvent("Play_Manipulation", gameObject);
        if (rotating)
        {
            rotating = false;
            currentRotationSpeed = rotationSpeed;
            transform.rotation = rotationAfter;
        }
        beforeZoomPosition.y += 1f;
        gameObject.transform.position = beforeZoomPosition;
        rigidBody.useGravity = true;
        zoom = false;
        transform.parent = inventory;
    }
}
