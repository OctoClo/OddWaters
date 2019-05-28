using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ERotation { R0, R90, R180 };

public class Interactible : MonoBehaviour
{
    [Header("General")]
    [SerializeField]
    AK.Wwise.Switch soundMaterial;
    [SerializeField]
    TextAsset transcriptJSONRecto;
    [SerializeField]
    TextAsset transcriptJSONVerso;
    Transcript transcriptRecto;
    Transcript transcriptVerso;
    bool switchTranscriptSide;
    int side;

    [Header("Offsets")]
    [SerializeField]
    [Range(2, 20)]
    int zoomOffset = 4;
    [SerializeField]
    [Range(0.01f, 0.5f)]
    float grabOffset = 0.1f;

    [Header("Rotations")]
    [SerializeField]
    [Range(1, 4)]
    float rotationSpeed = 3f;
    [Tooltip("Ordre X - Y - Z")]
    public ERotation[] rotationsAmount = new ERotation[3];

    [Header("References")]
    public InspectionInterface inspectionInterface;
    public TutorialManager tutorialManager;

    [HideInInspector]
    public bool grabbable = false;

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

    void Start()
    {
        mainCamera = Camera.main;
        rigidBody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        rotating = false;
        currentRotationSpeed = rotationSpeed;
        inventory = transform.parent;
        soundMaterial.SetValue(gameObject);

        if (transcriptJSONRecto != null)
            transcriptRecto = JsonUtility.FromJson<Transcript>(transcriptJSONRecto.text);
        if (transcriptJSONVerso != null)
            transcriptVerso = JsonUtility.FromJson<Transcript>(transcriptJSONVerso.text);

        switchTranscriptSide = false;
        side = 0;
    }

    public virtual void Trigger()
    {

    }

    public bool IsGrabbable()
    {
        return (rigidBody.velocity.sqrMagnitude <= 0.5f);
    }

    public void Grab()
    {
        AkSoundEngine.PostEvent("Play_Manipulation", gameObject);
        rigidBody.useGravity = false;
        transform.position = GetGrabbedPosition();
    }

    public Vector3 GetGrabbedPosition()
    {
        Vector3 position = mainCamera.transform.position - gameObject.transform.position;
        position.Normalize();
        position += transform.position;
        position.y = grabOffset;
        return position;
    }

    public void MoveTo(Vector3 newPosition)
    {
        rigidBody.velocity = (newPosition - transform.position) * 15;
    }

    public void Drop()
    {
        AkSoundEngine.PostEvent("Play_Manipulation", gameObject);
        rigidBody.useGravity = true;
    }

    public void Rotate(int axis, int direction)
    {
        int angle = getRotation(axis);

        if (angle != 0)
        {
            AkSoundEngine.PostEvent("Play_Manipulation", gameObject);
            boxCollider.enabled = false;

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

            rotationAfter = Quaternion.AngleAxis(angle * direction, axisVec) * rotationBefore;

            if (zoom)
            {
                inspectionInterface.SetButtonsInteractable(false);
                if (axis == 2 && angle == 180)
                    switchTranscriptSide = true;
            }

            if (tutorialManager.step == ETutorialStep.OBJECT_ROTATE)
                tutorialManager.CompleteStep();
        }
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
                boxCollider.enabled = true;
                inspectionInterface.SetButtonsInteractable(true);

                if (switchTranscriptSide)
                {
                    switchTranscriptSide = false;
                    side = (side + 1) % 2;
                    inspectionInterface.DisplayTranscriptSide(side);
                }
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
        zoomPosition = new Vector3(mainCamera.transform.position.x, beforeZoomPosition.y + zoomOffset, 0);
        gameObject.transform.position = zoomPosition;

        inspectionInterface.InitializeInterface(transcriptRecto, transcriptVerso, side);
        for (int i = 0; i < 3; i++)
        {
            if (rotationsAmount[i] == ERotation.R0)
                inspectionInterface.DeactivateAxis(i);
        }
        inspectionInterface.InitializeButtons();
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
        beforeZoomPosition.y += 0.5f;
        gameObject.transform.position = beforeZoomPosition;
        rigidBody.useGravity = true;
        zoom = false;
        transform.parent = inventory;

        if (tutorialManager.step == ETutorialStep.WAITING)
            tutorialManager.NextStep();
    }
}
