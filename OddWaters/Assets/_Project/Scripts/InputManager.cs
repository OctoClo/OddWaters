using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockInputEvent : GameEvent { public bool block; }

enum EInteractibleState { UNKNOWN, CLICKED, DRAGNDROP };

public class InputManager : MonoBehaviour
{
    // General
    [SerializeField]
    TutorialManager tutorialManager;
    [SerializeField]
    ScreenManager screenManager;
    [SerializeField]
    DialogueManager dialogueManager;
    Camera mainCamera;
    GameObject mouseProjection;
    RaycastHit[] hitsOnRayToMouse;
    bool blockInput;
    bool dialogueOngoing;

    // Tutorial
    [HideInInspector]
    public bool tutorial;
    bool firstTelescopeMove;

    // Interactible
    [SerializeField]
    InspectionInterface inspectionInterface;
    [SerializeField]
    GameObject rotationPanel;
    EventSystem eventSystem;
    Interactible interactible;
    Vector3 interactibleScreenPos;
    Vector3 interactibleOffset;
    EInteractibleState interactibleState;
    float interactiblePressTime;
    float interactibleClickTime;
    
    // Telescope
    [SerializeField]
    Telescope telescope;
    bool telescopeDrag;
    Vector3 dragBeginPos;
    Vector3 dragCurrentPos;
    float dragSpeed;
    [SerializeField]
    Animator upPartAnimator;

    // Map
    [SerializeField]
    NavigationManager navigationManager;
    [SerializeField]
    Boat boat;
    [SerializeField]
    Animator boatAnimator;
    bool navigation;

    
    void Start()
    {
        mouseProjection = new GameObject("Mouse Projection");
        mouseProjection.tag = "MouseProjection";
        BoxCollider mouseCollider = mouseProjection.AddComponent<BoxCollider>();
        mouseCollider.isTrigger = true;
        mouseCollider.size = new Vector3(0.5f, 1, 0.5f);
        boat.mouseProjection = mouseProjection;

        mainCamera = Camera.main;
        blockInput = false;
        dialogueOngoing = false;

        interactiblePressTime = 0;
        interactibleClickTime = 0.15f;
        eventSystem = EventSystem.current;

        telescopeDrag = false;
        navigation = false;
        firstTelescopeMove = true;
    }

    void OnEnable()
    {
        EventManager.Instance.AddListener<BlockInputEvent>(OnBlockInputEvent);
        EventManager.Instance.AddListener<DialogueEvent>(OnDialogueEvent);
    }

    void OnDisable()
    {
        EventManager.Instance.RemoveListener<BlockInputEvent>(OnBlockInputEvent);
        EventManager.Instance.RemoveListener<DialogueEvent>(OnDialogueEvent);
    }

    void Update()
    {
        //Debug.Log("Left Ctrl: " + Input.GetKey(KeyCode.LeftControl));
        //Debug.Log("Right Ctrl: " + Input.GetKey(KeyCode.RightControl));

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y));
        hitsOnRayToMouse = Physics.RaycastAll(ray);

        RaycastHit desk = hitsOnRayToMouse.FirstOrDefault(hit => hit.collider.CompareTag("Desk"));
        if (desk.collider)
            mouseProjection.transform.position = desk.point;

        if (!blockInput)
        {
            // Left button down
            if (Input.GetMouseButtonDown(0))
                HandleMouseLeftButtonDown();

            // Left button up
            if (Input.GetMouseButtonUp(0))
                HandleMouseLeftButtonUp();

            // Right button down
            if (Input.GetMouseButtonDown(1))
            {
                AkSoundEngine.PostEvent("Play_Click", gameObject);
                if (navigation)
                    StopNavigation();
                else if (interactibleState == EInteractibleState.CLICKED)
                    ExitInterfaceRotation();
            }

            // Interactible grab / rotate
            if (interactible)
            {
                interactiblePressTime += Time.deltaTime;
                if (interactibleState == EInteractibleState.UNKNOWN && interactiblePressTime > interactibleClickTime && (!tutorial || tutorialManager.step >= ETutorialStep.OBJECT_MOVE))
                {
                    interactibleState = EInteractibleState.DRAGNDROP;
                    CursorManager.Instance.SetCursor(ECursor.DRAG);
                    interactible.Grab();
                }

                if (interactibleState == EInteractibleState.DRAGNDROP && !interactible.rotating)
                {
                    if (Input.GetKeyDown(KeyCode.S))
                        interactible.Rotate(0, -1);
                    else if (Input.GetKeyDown(KeyCode.Z))
                        interactible.Rotate(0, 1);
                    else if (Input.GetKeyDown(KeyCode.E))
                        interactible.Rotate(1, 1);
                    else if (Input.GetKeyDown(KeyCode.A))
                        interactible.Rotate(1, -1);
                    else if (Input.GetKeyDown(KeyCode.D))
                        interactible.Rotate(2, 1);
                    else if (Input.GetKeyDown(KeyCode.Q))
                        interactible.Rotate(2, -1);
                }
            }

            // Hover things
            if (!interactible && !telescopeDrag && !navigation)
            {
                if (hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("Boat")) && (!tutorial || tutorialManager.step == ETutorialStep.BOAT_MOVE || tutorialManager.step == ETutorialStep.GO_TO_ISLAND))
                {
                    CursorManager.Instance.SetCursor(ECursor.HOVER);
                    boatAnimator.SetBool("Hover", true);
                }
                else
                {
                    boatAnimator.SetBool("Hover", false);

                    if (hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("TelescopeCollider")) && (!tutorial || tutorialManager.step == ETutorialStep.TELESCOPE_MOVE || tutorialManager.step == ETutorialStep.TELESCOPE_ZOOM))
                    {
                        CursorManager.Instance.SetCursor(ECursor.HOVER);
                        upPartAnimator.SetBool("Hover", true);

                        // Telescope zoom
                        if (Input.GetAxis("Mouse ScrollWheel") != 0 && (!tutorial || tutorialManager.step == ETutorialStep.TELESCOPE_ZOOM))
                        {
                            telescope.Zoom(Input.GetAxis("Mouse ScrollWheel"));
                        }
                    }
                    else if (hitsOnRayToMouse.Any(hit => hit.collider.GetComponent<Interactible>()) && (!tutorial || tutorialManager.step >= ETutorialStep.OBJECT_ZOOM))
                    {
                        CursorManager.Instance.SetCursor(ECursor.HOVER);
                    }
                        
                    else
                    {
                        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
                        upPartAnimator.SetBool("Hover", false);
                    }
                        
                }
            }
        }

        // Dialogue
        if (dialogueOngoing && Input.GetMouseButtonDown(0))
            dialogueManager.NextLine();

        // Telescope drag
        if (telescopeDrag)
        {
            dragCurrentPos = Input.mousePosition;
            dragSpeed = -(dragCurrentPos - dragBeginPos).x * Time.deltaTime;
            telescope.UpdateSpeed(dragSpeed);
        }

        // Navigation
        if (navigation)
            navigationManager.UpdateNavigation(mouseProjection.transform.position);
    }

    void HandleMouseLeftButtonDown()
    {
        AkSoundEngine.PostEvent("Play_Click", gameObject);
        hitsOnRayToMouse = hitsOnRayToMouse.OrderBy(hit => Vector3.SqrMagnitude(mainCamera.transform.position - hit.point)).ToArray();
        if (hitsOnRayToMouse.Length > 0)
        {
            if (interactibleState != EInteractibleState.CLICKED)
            {
                // Launch navigation
                if (hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("Boat")) && (!tutorial || tutorialManager.step == ETutorialStep.BOAT_MOVE || tutorialManager.step == ETutorialStep.GO_TO_ISLAND))
                {
                    navigation = true;
                    boatAnimator.SetBool("Hold", true);
                    boat.StartTargeting();
                }
                else
                {
                    // Interactible
                    RaycastHit hitInfo = hitsOnRayToMouse.FirstOrDefault(hit => hit.collider.GetComponent<Interactible>());
                    if (hitInfo.collider && hitInfo.collider.GetComponent<Interactible>().IsGrabbable() && (!tutorial || tutorialManager.step >= ETutorialStep.OBJECT_ZOOM))
                    {
                        interactible = hitInfo.collider.GetComponent<Interactible>();
                        interactiblePressTime = Time.time;
                        interactibleState = EInteractibleState.UNKNOWN;
                        interactiblePressTime = 0;
                        Vector3 interactibleGrabbedPos = interactible.GetGrabbedPosition();
                        interactibleScreenPos = mainCamera.WorldToScreenPoint(interactibleGrabbedPos);
                        interactibleOffset = interactibleGrabbedPos - mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, interactibleScreenPos.z));
                    }
                    else
                    {
                        // Telescope
                        telescopeDrag = (hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("TelescopeCollider")) && (!tutorial || tutorialManager.step == ETutorialStep.TELESCOPE_MOVE || tutorialManager.step == ETutorialStep.TELESCOPE_ZOOM));
                        if (telescopeDrag)
                        {
                            dragBeginPos = Input.mousePosition;
                            Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y);
                            telescope.BeginDrag(mainCamera.ScreenToWorldPoint(mouseScreenPos));
                        }
                    }
                }
            }
            else
            {
                if (!eventSystem.IsPointerOverGameObject() && !hitsOnRayToMouse.Any(hit => hit.collider.gameObject.name == interactible.name))
                    ExitInterfaceRotation();
            }
        }
    }

    void HandleMouseLeftButtonUp()
    {
        if (interactible)
        {
            if (interactibleState == EInteractibleState.DRAGNDROP)
            {
                CursorManager.Instance.SetCursor(ECursor.DEFAULT);
                interactibleState = EInteractibleState.UNKNOWN;
                interactible.Drop();
                interactible = null;

                if (tutorialManager.step == ETutorialStep.OBJECT_MOVE)
                    tutorialManager.NextStep();
            }
            else if (interactibleState == EInteractibleState.UNKNOWN)
            {
                CursorManager.Instance.SetCursor(ECursor.DEFAULT);
                interactibleState = EInteractibleState.CLICKED;
                interactible.EnterRotationInterface();
                inspectionInterface.gameObject.SetActive(true);
                rotationPanel.SetActive(true);
                telescope.SetImageAlpha(true);
                boat.SetImageAlpha(true);

                if (tutorialManager.step == ETutorialStep.OBJECT_ZOOM)
                    tutorialManager.NextStep();
            }
        }
        else if (telescopeDrag)
        {
            telescopeDrag = false;
            telescope.EndDrag();
            if (tutorialManager.step == ETutorialStep.TELESCOPE_MOVE && firstTelescopeMove)
                StartCoroutine(WaitBeforeTutorialGoOn());
        }
        else if (navigation)
        {
            navigationManager.Navigate();
            StopNavigation();
        }
    }

    IEnumerator WaitBeforeTutorialGoOn()
    {
        firstTelescopeMove = false;
        yield return new WaitForSeconds(tutorialManager.telescopeDragWait);
        if (!telescopeDrag)
            tutorialManager.NextStep();
        else
            firstTelescopeMove = true;
    }

    void FixedUpdate()
    {
        // Interactible drag and drop - Check desk borders before moving
        if (interactible && interactibleState == EInteractibleState.DRAGNDROP)
        {
            Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, interactibleScreenPos.z);
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos) + interactibleOffset;
            interactible.MoveTo(mouseWorldPos);
        }
    }

    void ExitInterfaceRotation()
    {
        interactibleState = EInteractibleState.UNKNOWN;
        inspectionInterface.gameObject.SetActive(false);
        rotationPanel.SetActive(false);
        telescope.SetImageAlpha(false);
        boat.SetImageAlpha(false);
        interactible.ExitRotationInterface();
        interactible = null;
    }

    void StopNavigation()
    {
        navigation = false;
        boatAnimator.SetBool("Hold", false);
        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
        boat.StopTargeting();
    }

    void OnBlockInputEvent(BlockInputEvent e)
    {
        if (e.block)
            CursorManager.Instance.SetCursor(ECursor.DEFAULT);
        blockInput = e.block;
    }

    void OnDialogueEvent(DialogueEvent e)
    {
        dialogueOngoing = e.ongoing;
    }

    public void RotateButtonPositive(int axis)
    {
        interactible.Rotate(axis, 1);
    }

    public void RotateButtonNegative(int axis)
    {
        interactible.Rotate(axis, -1);
    }
}
