using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockInputEvent : GameEvent { public bool block; public bool navigation; }

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
    bool navigating;
    bool dialogueOngoing;

    // Double click
    [SerializeField]
    float delayBetweenDoubleClick = 0.12f;
    float timerDoubleClick;
    bool firstClickTelescope;
    bool firstClickInteractible;
    bool interactibleAlreadyDropped;

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
    
    // Telescope
    [SerializeField]
    Telescope telescope;
    bool telescopeDrag;
    Vector3 dragBeginPos;
    Vector3 dragCurrentPos;
    float dragSpeed;

    // Animation
    [SerializeField]
    Animator globalAnimator;
    [SerializeField]
    BoatTrailAnimation trailAnimation;

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
        eventSystem = EventSystem.current;
        blockInput = false;
        navigating = false;
        dialogueOngoing = false;
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

        // Left button down
        if (Input.GetMouseButtonDown(0))
            HandleMouseLeftButtonDown();
        
        // Telescope single click
        if (firstClickTelescope && (Time.time - timerDoubleClick) > delayBetweenDoubleClick)
        {
            firstClickTelescope = false;
            if (!tutorial || tutorialManager.step == ETutorialStep.TELESCOPE_MOVE || tutorialManager.step == ETutorialStep.TELESCOPE_ZOOM)
            {
                if (telescopeDrag)
                    EndTelescopeDrag();

                telescopeDrag = true;
                dragBeginPos = Input.mousePosition;
                Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y);
                telescope.BeginDrag(mainCamera.ScreenToWorldPoint(mouseScreenPos));
            }
        }

        // Interactible single click
        if (firstClickInteractible && (Time.time - timerDoubleClick) > delayBetweenDoubleClick)
        {
            firstClickInteractible = false;
            if (interactibleState == EInteractibleState.UNKNOWN &&  (!tutorial || tutorialManager.step >= ETutorialStep.OBJECT_MOVE))
            {
                interactibleState = EInteractibleState.DRAGNDROP;
                CursorManager.Instance.SetCursor(ECursor.DRAG);
                interactible.Grab();

                if (interactibleAlreadyDropped)
                {
                    interactibleAlreadyDropped = false;
                    DropInteractible();
                }
            }
            else
            {
                CursorManager.Instance.SetCursor(ECursor.DEFAULT);
                interactibleState = EInteractibleState.UNKNOWN;
                interactible = null;
            }
        }

        // Left button up
        if (Input.GetMouseButtonUp(0))
            HandleMouseLeftButtonUp();

        // Right button down
        if (Input.GetMouseButtonDown(1))
        {
            AkSoundEngine.PostEvent("Play_Click", gameObject);
            if (navigation)
                StopNavigation();
            else if ((!blockInput || navigating) && interactibleState == EInteractibleState.CLICKED)
                ExitInterfaceRotation();
        }

        // Interactible rotate
        if ((!blockInput || navigating) && interactible && interactibleState == EInteractibleState.DRAGNDROP && !interactible.rotating)
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

        // Hover things
        if (!interactible && !telescopeDrag && !navigation)
        {
            // Hover boat
            if (!blockInput && (!tutorial || tutorialManager.step == ETutorialStep.BOAT_MOVE || tutorialManager.step == ETutorialStep.GO_TO_ISLAND) && hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("Boat")))
            {
                CursorManager.Instance.SetCursor(ECursor.HOVER);
                boatAnimator.SetBool("Hover", true);
            }
            else
            {
                boatAnimator.SetBool("Hover", false);

                // Hover up part
                if (!blockInput && (!tutorial || tutorialManager.step == ETutorialStep.TELESCOPE_MOVE || tutorialManager.step == ETutorialStep.TELESCOPE_ZOOM) && hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("UpPartCollider")))
                {
                    if (!globalAnimator.GetBool("Hover"))
                        trailAnimation.Hover();
                    globalAnimator.SetBool("Hover", true);

                    if (telescope.gameObject.activeInHierarchy)
                    {
                        CursorManager.Instance.SetCursor(ECursor.HOVER);

                        // Telescope zoom
                        if (Input.GetAxis("Mouse ScrollWheel") != 0 && telescope.gameObject.activeInHierarchy && (!tutorial || tutorialManager.step == ETutorialStep.TELESCOPE_ZOOM))
                            telescope.Zoom(Input.GetAxis("Mouse ScrollWheel"));
                    }
                    else if (hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("Character")))
                        CursorManager.Instance.SetCursor(ECursor.HOVER);
                    else
                        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
                }
                else if ((!blockInput || navigating) && (!tutorial || tutorialManager.step >= ETutorialStep.OBJECT_ZOOM) && hitsOnRayToMouse.Any(hit => hit.collider.GetComponent<Interactible>()))
                {
                    // Hover interactible
                    CursorManager.Instance.SetCursor(ECursor.HOVER);
                }
                else if (!blockInput || navigating)
                {
                    CursorManager.Instance.SetCursor(ECursor.DEFAULT);

                    if (globalAnimator.GetBool("Hover"))
                        trailAnimation.Hover();
                    globalAnimator.SetBool("Hover", false);
                }
            }
        }

        // Dialogue
        if (dialogueOngoing && Input.GetMouseButtonDown(0))
            dialogueManager.NextLine();

        // Telescope drag
        if (telescopeDrag)
        {
            if (!Input.GetMouseButton(0))
                EndTelescopeDrag();
            else
            {
                dragCurrentPos = Input.mousePosition;
                dragSpeed = -(dragCurrentPos - dragBeginPos).x * Time.deltaTime;
                telescope.UpdateSpeed(dragSpeed);
            }
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
                if (!blockInput && (!tutorial || tutorialManager.step == ETutorialStep.BOAT_MOVE || tutorialManager.step == ETutorialStep.GO_TO_ISLAND) && hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("Boat")))
                {
                    navigation = true;
                    boatAnimator.SetBool("Hold", true);
                    boat.StartTargeting();
                }
                else
                {
                    // Interactible
                    RaycastHit hitInfo = hitsOnRayToMouse.FirstOrDefault(hit => hit.collider.GetComponent<Interactible>());
                    if ((!blockInput || navigating) && hitInfo.collider && (!tutorial || tutorialManager.step >= ETutorialStep.OBJECT_ZOOM) && hitInfo.collider.GetComponent<Interactible>().IsGrabbable())
                    {
                        if (!firstClickInteractible)
                        {
                            firstClickInteractible = true;
                            timerDoubleClick = Time.time;
                            interactible = hitInfo.collider.GetComponent<Interactible>();
                            interactibleState = EInteractibleState.UNKNOWN;
                            Vector3 interactibleGrabbedPos = interactible.GetGrabbedPosition();
                            interactibleScreenPos = mainCamera.WorldToScreenPoint(interactibleGrabbedPos);
                            interactibleOffset = interactibleGrabbedPos - mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, interactibleScreenPos.z));
                        }
                        else
                        {
                            firstClickInteractible = false;
                            CursorManager.Instance.SetCursor(ECursor.DEFAULT);
                            interactibleState = EInteractibleState.CLICKED;
                            interactible.EnterRotationInterface();
                            inspectionInterface.gameObject.SetActive(true);
                            rotationPanel.SetActive(true);
                            telescope.SetImageAlpha(true);
                            boat.SetImageAlpha(true);

                            if (tutorialManager.step == ETutorialStep.OBJECT_ZOOM)
                                tutorialManager.CompleteStep();
                        }
                    }
                    else if (!blockInput)
                    {
                        // Telescope
                        bool telescopeClick = (telescope.gameObject.activeInHierarchy &&
                            (!tutorial || tutorialManager.step == ETutorialStep.TELESCOPE_MOVE || tutorialManager.step == ETutorialStep.TELESCOPE_ZOOM) &&
                            hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("UpPartCollider")));
                        if (telescopeClick)
                        {
                            if (!firstClickTelescope)
                            {
                                firstClickTelescope = true;
                                timerDoubleClick = Time.time;
                            }
                            else
                            {
                                firstClickTelescope = false;
                                if (!tutorial || tutorialManager.step == ETutorialStep.TELESCOPE_ZOOM)
                                    telescope.ChangeZoom();
                            }
                        }
                        else if (!tutorial && hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("Character")))
                        {
                            // Character
                            StartCoroutine(screenManager.RelaunchDialogue());
                            StartCoroutine(WaitBeforeResettingHoverTrigger());
                        }
                    }
                }
            }
            else if(!eventSystem.IsPointerOverGameObject() && !hitsOnRayToMouse.Any(hit => hit.collider.gameObject.name == interactible.name))
                    ExitInterfaceRotation();
        }
    }

    IEnumerator WaitBeforeResettingHoverTrigger()
    {
        yield return new WaitForSeconds(0.2f);
        globalAnimator.SetBool("Hover", false);
    }

    void HandleMouseLeftButtonUp()
    {
        if (interactible)
        {
            if (interactibleState == EInteractibleState.UNKNOWN)
                interactibleAlreadyDropped = true;
            else if (interactibleState == EInteractibleState.DRAGNDROP)
                DropInteractible();
        }
        else if (telescopeDrag)
            EndTelescopeDrag();
        else if (navigation)
        {
            navigationManager.Navigate();
            StopNavigation();
        }
    }

    void EndTelescopeDrag()
    {
        firstClickTelescope = false;
        telescopeDrag = false;
        telescope.EndDrag();
        if (tutorialManager.step == ETutorialStep.TELESCOPE_MOVE && firstTelescopeMove)
            StartCoroutine(WaitBeforeTutorialGoOn());
    }

    void DropInteractible()
    {
        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
        interactibleState = EInteractibleState.UNKNOWN;
        interactible.Drop();
        interactible = null;

        if (tutorialManager.step == ETutorialStep.OBJECT_MOVE)
            tutorialManager.CompleteStep();
    }

    IEnumerator WaitBeforeTutorialGoOn()
    {
        firstTelescopeMove = false;
        yield return new WaitForSeconds(tutorialManager.telescopeDragWait);
        if (!telescopeDrag)
            tutorialManager.CompleteStep();
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

    public void ExitInterfaceRotation()
    {
        interactibleState = EInteractibleState.UNKNOWN;
        inspectionInterface.gameObject.SetActive(false);
        rotationPanel.SetActive(false);
        telescope.SetImageAlpha(false);
        boat.SetImageAlpha(false);
        interactible.ExitRotationInterface();
        interactible = null;

        if (tutorialManager.step == ETutorialStep.WAITING)
            tutorialManager.CompleteStep();
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
        blockInput = e.block;
        navigating = e.navigation;
        if (e.block)
            CursorManager.Instance.SetCursor(ECursor.DEFAULT);
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

