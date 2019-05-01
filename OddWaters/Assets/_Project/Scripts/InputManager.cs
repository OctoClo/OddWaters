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
    ScreenManager screenManager;
    Camera mainCamera;
    GameObject mouseProjection;
    RaycastHit[] hitsOnRayToMouse;

    // Desk
    [SerializeField]
    PanZone[] panZones;
    [HideInInspector]
    public bool mouseProjectionOutOfDesk;

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
    bool blockInput;

    // Telescope
    [SerializeField]
    Telescope telescope;
    Vector3 dragBeginPos;
    bool telescopeDrag;

    // Map
    [SerializeField]
    NavigationManager navigationManager;
    [SerializeField]
    Boat boat;
    bool navigation;
    
    void Start()
    {
        mouseProjection = new GameObject("Mouse Projection");
        mouseProjection.tag = "MouseProjection";
        BoxCollider mouseCollider = mouseProjection.AddComponent<BoxCollider>();
        mouseCollider.isTrigger = true;
        mouseCollider.size = new Vector3(0.5f, 1, 0.5f);
        boat.mouseProjection = mouseProjection;

        mouseProjectionOutOfDesk = false;

        mainCamera = Camera.main;
        blockInput = false;
        
        interactiblePressTime = 0;
        interactibleClickTime = 0.15f;
        eventSystem = EventSystem.current;

        telescopeDrag = false;
        navigation = false;
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
                if (navigation)
                    StopNavigation();
                else if (interactibleState == EInteractibleState.CLICKED)
                    ExitInterfaceRotation();
            }

            // Mouse wheel
            if (!navigation && !telescopeDrag && interactibleState != EInteractibleState.CLICKED && Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                if (hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("TelescopeCollider") || hit.collider.GetComponent<TelescopeElement>()))
                    telescope.Zoom(Input.GetAxis("Mouse ScrollWheel"));
            }

            // Interactible grab / rotate
            if (interactible)
            {
                interactiblePressTime += Time.deltaTime;
                if (interactibleState == EInteractibleState.UNKNOWN && interactiblePressTime > interactibleClickTime)
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

            if (!interactible && !telescopeDrag && !navigation)
            {
                if (hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("Boat") || hit.collider.GetComponent<Interactible>()))
                    CursorManager.Instance.SetCursor(ECursor.HOVER);
                else
                    CursorManager.Instance.SetCursor(ECursor.DEFAULT);
            }
        }

        // Telescope drag
        if (telescopeDrag)
        {
            Vector3 dragCurrentPos = Input.mousePosition;
            telescope.UpdateSpeed(-(dragCurrentPos - dragBeginPos).x * Time.deltaTime);
        }

        // Navigation
        if (navigation)
            navigationManager.UpdateNavigation(mouseProjection.transform.position);
    }

    void HandleMouseLeftButtonDown()
    {
        hitsOnRayToMouse = hitsOnRayToMouse.OrderBy(hit => Vector3.SqrMagnitude(mainCamera.transform.position - hit.point)).ToArray(); ;
        if (hitsOnRayToMouse.Length > 0)
        {
            if (interactibleState != EInteractibleState.CLICKED)
            {
                if (navigation)
                {
                    StopNavigation();
                    RaycastHit hitInfo = hitsOnRayToMouse.FirstOrDefault(hit => hit.collider.GetComponent<Island>() != null);
                    hitInfo = hitsOnRayToMouse.FirstOrDefault(hit => hit.collider.GetComponentInParent<MapZone>() != null);
                    if (hitInfo.collider)
                    {
                        ENavigationResult result = navigationManager.GetNavigationResult(mouseProjection.transform.position);
                        if (result != ENavigationResult.KO)
                            navigationManager.NavigateToPosition(mouseProjection.transform.position, hitInfo.collider.GetComponentInParent<MapZone>().zoneNumber);
                    }
                }
                else
                {
                    // Launch navigation
                    if (hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("Boat")))
                    {
                        navigation = true;
                        ActivatePanZones(false);
                        boat.StartTargeting();
                    }
                    else
                    {
                        // Interactible
                        RaycastHit hitInfo = hitsOnRayToMouse.FirstOrDefault(hit => hit.collider.GetComponent<Interactible>());
                        if (hitInfo.collider && hitInfo.collider.GetComponent<Interactible>().IsGrabbable())
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
                            telescopeDrag = hitsOnRayToMouse.Any(hit => hit.collider.CompareTag("TelescopeCollider"));
                            if (telescopeDrag)
                            {
                                dragBeginPos = Input.mousePosition;
                                Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y);
                                telescope.BeginDrag(mainCamera.ScreenToWorldPoint(mouseScreenPos));
                                ActivatePanZones(false);
                            }
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
            }
            else if (interactibleState == EInteractibleState.UNKNOWN)
            {
                CursorManager.Instance.SetCursor(ECursor.DEFAULT);
                interactibleState = EInteractibleState.CLICKED;
                interactible.EnterRotationInterface();
                inspectionInterface.gameObject.SetActive(true);
                rotationPanel.SetActive(true);
                telescope.SetImageAlpha(true);
                ActivatePanZones(false);
            }
        }
        else if (telescopeDrag)
        {
            telescopeDrag = false;
            telescope.EndDrag();
            ActivatePanZones(true);
        }
    }

    void FixedUpdate()
    {
        // Interactible drag and drop - Check desk borders before moving
        if (interactible && interactibleState == EInteractibleState.DRAGNDROP && !mouseProjectionOutOfDesk)
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
        interactible.ExitRotationInterface();
        ActivatePanZones(true);
        interactible = null;
    }

    void StopNavigation()
    {
        navigation = false;
        ActivatePanZones(true);
        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
        boat.StopTargeting();
    }

    void ActivatePanZones(bool active)
    {
        panZones[0].gameObject.SetActive(active);
        panZones[1].gameObject.SetActive(active);
    }

    void OnBlockInputEvent(BlockInputEvent e)
    {
        blockInput = e.block;
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
