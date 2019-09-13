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
    [Header("General")]
    // Double click
    [SerializeField]
    float delayBetweenDoubleClick = 0.12f;
    float timerDoubleClick;
    bool firstClickTelescope;
    bool firstClickInteractible;
    bool interactibleAlreadyDropped;

    [Header("References")]
    [SerializeField]
    TutorialManager tutorialManager;
    [SerializeField]
    ScreenManager screenManager;
    [SerializeField]
    DialogueManager dialogueManager;
    [SerializeField]
    NavigationManager navigationManager;
    [SerializeField]
    GameObject pauseObject;
    [SerializeField]
    GameObject controlsObject;

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

    [SerializeField]
    Animator globalAnimator;

    // Boat
    [SerializeField]
    Boat boat;
    [SerializeField]
    Animator boatAnimator;
    bool navigation;

    // Telescope
    [SerializeField]
    Telescope telescope;
    bool telescopeDrag;
    bool telescopeDragFromWheel;
    Vector3 dragBeginPos;
    Vector3 dragCurrentPos;
    float dragSpeed;

    // Tutorial
    [HideInInspector]
    public bool tutorial;
    bool firstTelescopeMove;

    Camera mainCamera;
    GameObject mouseProjection;
    RaycastHit[] hitsOnRayToMouse;
    bool blockInput;
    bool navigating;
    bool pause;

    void Start()
    {
        OptionsManager.Instance.UpdateTranslator();

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
        navigation = false;
        firstTelescopeMove = true;
        telescopeDrag = false;
        telescopeDragFromWheel = false;
        pause = false;
    }

    void OnEnable()
    {
        EventManager.Instance.AddListener<BlockInputEvent>(OnBlockInputEvent);
    }

    void OnDisable()
    {
        EventManager.Instance.RemoveListener<BlockInputEvent>(OnBlockInputEvent);
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
        if (!eventSystem.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
            HandleMouseLeftButtonDown();

        // Left button up
        if (Input.GetMouseButtonUp(0))
            HandleMouseLeftButtonUp();

        // Telescope single click
        if (firstClickTelescope)
        {
            float timeSinceClick = Time.time - timerDoubleClick;
            if (timeSinceClick > delayBetweenDoubleClick || Input.GetMouseButton(0) && timeSinceClick > 0.1f)
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
        }

        // Begin wheel telescope drag
        if (Input.GetMouseButtonDown(2) && telescope.gameObject.activeInHierarchy && !telescopeDrag &&
            (!tutorial || tutorialManager.step == ETutorialStep.TELESCOPE_MOVE || tutorialManager.step == ETutorialStep.TELESCOPE_ZOOM) &&
            hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("UpPartCollider")))
        {
            telescopeDrag = true;
            telescopeDragFromWheel = true;
            dragBeginPos = Input.mousePosition;
            Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y);
            telescope.BeginDrag(mainCamera.ScreenToWorldPoint(mouseScreenPos));
        }

        // End wheel telescope drag
        if (Input.GetMouseButtonUp(2) && telescopeDrag && telescopeDragFromWheel)
            EndTelescopeDrag();

        // Right button down
        if (Input.GetMouseButtonDown(1))
        {
            AkSoundEngine.PostEvent("Play_Click", gameObject);

            if (navigation)
                StopNavigation();
            else
            {
                if (interactibleState == EInteractibleState.CLICKED)
                    ExitInterfaceRotation();
                else
                {
                    hitsOnRayToMouse = hitsOnRayToMouse.OrderBy(hit => Vector3.SqrMagnitude(mainCamera.transform.position - hit.point)).ToArray();
                    RaycastHit hitInfo = hitsOnRayToMouse.FirstOrDefault(hit => hit.collider.GetComponent<Interactible>());
                    if ((!blockInput || navigating) && hitInfo.collider && (!tutorial || tutorialManager.step >= ETutorialStep.OBJECT_ZOOM) && hitInfo.collider.GetComponent<Interactible>().IsGrabbable())
                    {
                        interactible = hitInfo.collider.GetComponent<Interactible>();
                        interactibleState = EInteractibleState.CLICKED;
                        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
                        interactible.EnterRotationInterface();
                        inspectionInterface.gameObject.SetActive(true);
                        rotationPanel.SetActive(true);
                        telescope.SetImageAlpha(true);
                        boat.SetImageAlpha(true);

                        if (tutorialManager.step == ETutorialStep.OBJECT_ZOOM)
                            tutorialManager.CompleteStep();
                    }
                }
            }
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

        // Pause
        if (Input.GetKeyDown(KeyCode.Escape) && !interactible && !telescopeDrag && !navigation)
            ToggleOptions();

        // Hover things
        if (!eventSystem.IsPointerOverGameObject() && !interactible && !telescopeDrag && !navigation)
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
                    globalAnimator.SetBool("Hover", false);
                }
            }
        }

        // Telescope drag
        if (telescopeDrag)
        {
            if (!Input.GetMouseButton(0) && !Input.GetMouseButton(2))
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
                    if ((!blockInput || navigating) && hitInfo.collider && (!tutorial || tutorialManager.step >= ETutorialStep.OBJECT_MOVE) && hitInfo.collider.GetComponent<Interactible>().IsGrabbable())
                    {
                        interactible = hitInfo.collider.GetComponent<Interactible>();
                        interactibleState = EInteractibleState.DRAGNDROP;
                        CursorManager.Instance.SetCursor(ECursor.DRAG);
                        Vector3 interactibleGrabbedPos = interactible.GetGrabbedPosition();
                        interactibleScreenPos = mainCamera.WorldToScreenPoint(interactibleGrabbedPos);
                        interactibleOffset = interactibleGrabbedPos - mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, interactibleScreenPos.z));
                        interactible.Grab();
                    }
                    else if (!blockInput)
                    {
                        // Telescope
                        bool telescopeClick = (telescope.gameObject.activeInHierarchy && !telescopeDrag &&
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
        if (interactible && interactibleState == EInteractibleState.DRAGNDROP)
            DropInteractible();
        else if (telescopeDrag && !telescopeDragFromWheel)
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
        telescopeDragFromWheel = false;
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

        if (tutorialManager.step == ETutorialStep.OBJECT_ROTATE && tutorialManager.stateCompleted)
            tutorialManager.NextStep();
    }

    void StopNavigation()
    {
        navigation = false;
        boatAnimator.SetBool("Hold", false);
        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
        boat.StopTargeting();
    }

    public void StopCurrentInteractions()
    {
        if (interactible)
        {
            if (interactibleState == EInteractibleState.CLICKED)
                ExitInterfaceRotation();
            else if (interactibleState == EInteractibleState.DRAGNDROP)
                DropInteractible();
        }
    }

    void OnBlockInputEvent(BlockInputEvent e)
    {
        blockInput = e.block;
        navigating = e.navigation;
        if (e.block)
            CursorManager.Instance.SetCursor(ECursor.DEFAULT);
    }

    public void RotateButtonPositive(int axis)
    {
        interactible.Rotate(axis, 1);
    }

    public void RotateButtonNegative(int axis)
    {
        interactible.Rotate(axis, -1);
    }

    public void ToggleOptions()
    {
        if (controlsObject.activeInHierarchy)
            controlsObject.SetActive(false);
        else
        {
            pause = !pause;
            pauseObject.SetActive(pause);

            if (pause)
            {
                AkSoundEngine.PostEvent("Play_TelescopeOpen_UI", gameObject);
                AkSoundEngine.SetState("Pause", "Pause");
            }
            else
            {
                AkSoundEngine.PostEvent("Play_TelescopeClose_UI", gameObject);
                AkSoundEngine.SetState("Pause", "InGame");
            }
        }
    }

    public void MouseEnters()
    {
        CursorManager.Instance.SetCursor(ECursor.HOVER);
    }

    public void MouseExits()
    {
        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
    }
}

