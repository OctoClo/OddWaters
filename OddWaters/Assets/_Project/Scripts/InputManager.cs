using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BlockInputEvent : GameEvent { public bool block; }

enum EInteractibleState { UNKNOWN, CLICKED, DRAGNDROP };

public class InputManager : MonoBehaviour
{
    [SerializeField]
    NavigationManager navigationManager;

    [SerializeField]
    ScreenManager screenManager;

    [SerializeField]
    RotationInterface rotationInterface;
    [SerializeField]
    GameObject rotationPanel;

    GameObject mouseProjection;

    // Desk

    [SerializeField]
    PanZone[] panZones;

    [SerializeField]
    GameObject desk;
    float deskMinX;
    float deskMaxX;
    float deskMinZ;
    float deskMaxZ;

    Camera mainCamera;
    bool blockInput;

    Interactible interactible;
    Vector3 interactibleScreenPos;
    Vector3 interactibleOffset;
    EInteractibleState interactibleState;
    float interactiblePressTime;
    float interactibleClickTime;

    // Telescope

    [SerializeField]
    Telescope telescope;
    Vector3 dragBeginPos;
    bool telescopeDrag;

    // Map

    [SerializeField]
    [Tooltip("Dans l'ordre : mer, île, KO")]
    Sprite[] cursorsNavigation;
    Vector2 cursorOffset;
    bool navigation;
    
    void Start()
    {
        mouseProjection = new GameObject("Mouse Projection");
        mouseProjection.tag = "MouseProjection";
        BoxCollider mouseCollider = mouseProjection.AddComponent<BoxCollider>();
        mouseCollider.isTrigger = true;

        Vector3 position = desk.transform.position;
        Vector3 scale = desk.transform.localScale;
        deskMinX = position.x - scale.x / 2;
        deskMaxX = position.x + scale.x / 2;
        deskMinZ = position.z - scale.z / 2;
        deskMaxZ = position.z + scale.z / 2;

        mainCamera = Camera.main;
        blockInput = false;

        interactiblePressTime = 0;
        interactibleClickTime = 0.15f;

        telescopeDrag = false;

        cursorOffset = new Vector2(cursorsNavigation[0].texture.width / 2, cursorsNavigation[0].texture.height / 2);
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
        if (!blockInput)
        {
            if (Input.GetMouseButtonDown(0)) // Left button down
                HandleMouseLeftButtonDown();

            if (Input.GetMouseButtonUp(0)) // Left button up
                HandleMouseLeftButtonUp();

            if (Input.GetMouseButtonDown(1)) // Right button down
            {
                if (navigation)
                    StopNavigation();
                else if (interactibleState == EInteractibleState.CLICKED)
                {
                    interactibleState = EInteractibleState.UNKNOWN;
                    rotationInterface.ResetButtons();
                    rotationInterface.gameObject.SetActive(false);
                    rotationPanel.SetActive(false);
                    telescope.SetImageAlpha(false);
                    interactible.ExitRotationInterface();
                    panZones[0].gameObject.SetActive(true);
                    panZones[1].gameObject.SetActive(true);
                    interactible = null;
                }
            }

            if (!navigation && interactibleState != EInteractibleState.CLICKED && Input.GetAxis("Mouse ScrollWheel") != 0) // Mouse wheel
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits = Physics.RaycastAll(ray);
                if (hits.Length > 0)
                {
                    if (hits.Any(hit => hit.collider.CompareTag("TelescopeCollider") || hit.collider.GetComponent<TelescopeElement>()))
                        telescope.Zoom(Input.GetAxis("Mouse ScrollWheel"));
                }
            }

            if (interactible)
            {
                interactiblePressTime += Time.deltaTime;
                if (interactibleState == EInteractibleState.UNKNOWN && interactiblePressTime > interactibleClickTime)
                {
                    interactibleState = EInteractibleState.DRAGNDROP;
                    interactible.Grab();
                    interactibleScreenPos = mainCamera.WorldToScreenPoint(interactible.gameObject.transform.position);
                    interactibleOffset = interactible.gameObject.transform.position - mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, interactibleScreenPos.z));
                }

                if (interactibleState == EInteractibleState.DRAGNDROP)
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
        }

        if (telescopeDrag)
        {
            Vector3 dragCurrentPos = Input.mousePosition;
            telescope.UpdateSpeed(-(dragCurrentPos - dragBeginPos).x);
        }

        if (navigation)
        {
            Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y);
            int cursorType = (int)navigationManager.GetNavigationResult(mainCamera.ScreenToWorldPoint(mouseScreenPos));
            Cursor.SetCursor(cursorsNavigation[cursorType].texture, cursorOffset, CursorMode.Auto);
        }

        mouseProjection.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y));
    }

    void HandleMouseLeftButtonDown()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray).OrderBy(hit => Vector3.SqrMagnitude(hit.point - mainCamera.transform.position)).ToArray(); ;
        if (hits.Length > 0)
        {
            if (interactibleState != EInteractibleState.CLICKED)
            {
                if (navigation)
                {
                    RaycastHit hitInfo = hits.FirstOrDefault(hit => hit.collider.GetComponent<Island>() != null);
                    if (hitInfo.collider) // Island navigation
                    {
                        Island island = hitInfo.collider.GetComponent<Island>();
                        StopNavigation();
                        navigationManager.NavigateToIsland(island);
                    }
                    else
                    {
                        hitInfo = hits.FirstOrDefault(hit => hit.collider.transform.GetComponentInParent<MapZone>() != null);
                        if (hitInfo.collider) // Sea navigation
                        {
                            MapZone mapZone = hitInfo.collider.transform.GetComponentInParent<MapZone>();
                            if (mapZone.visible)
                            {
                                Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y);
                                if (navigationManager.GetNavigationResult(mainCamera.ScreenToWorldPoint(mouseScreenPos)) != ENavigationResult.KO)
                                {
                                    StopNavigation();
                                    if (screenManager.screenType == EScreenType.ISLAND_SMALL)
                                        screenManager.LeaveIsland();
                                    navigationManager.NavigateToZone(mainCamera.ScreenToWorldPoint(mouseScreenPos), mapZone.zoneNumber);
                                }
                            }
                            else
                            {
                                Debug.Log("Zone not yet available");
                            }
                        }
                        else
                        {
                            Debug.Log("Navigation is possible on map only");
                            StopNavigation();
                        }
                    }
                }
                else
                {
                    if (hits.Any(hit => hit.collider.CompareTag("Boat"))) // Navigation
                    {
                        navigation = true;
                    }
                    else
                    {
                        RaycastHit hitInfo = hits.FirstOrDefault(hit => hit.collider.GetComponent<Interactible>());
                        if (hitInfo.collider && hitInfo.collider.GetComponent<Rigidbody>().velocity == Vector3.zero) // Inventory
                        {
                            interactible = hitInfo.collider.GetComponent<Interactible>();
                            interactiblePressTime = Time.time;
                            interactibleState = EInteractibleState.UNKNOWN;
                            interactiblePressTime = 0;
                        }
                        else
                        {
                            telescopeDrag = hits.Any(hit => hit.collider.CompareTag("TelescopeCollider"));
                            if (telescopeDrag) // Telescope
                            {
                                dragBeginPos = Input.mousePosition;
                                Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y);
                                telescope.BeginDrag(mainCamera.ScreenToWorldPoint(mouseScreenPos));
                            }
                        }
                    }

                }
            }
        }
    }

    void HandleMouseLeftButtonUp()
    {
        if (interactible)
        {
            if (interactibleState == EInteractibleState.DRAGNDROP)
            {
                interactibleState = EInteractibleState.UNKNOWN;
                interactible.Drop();
                interactible = null;
            }
            else if (interactibleState == EInteractibleState.UNKNOWN)
            {
                interactibleState = EInteractibleState.CLICKED;
                interactible.EnterRotationInterface();
                rotationInterface.gameObject.SetActive(true);
                interactible.SetRotationInterfaceAxis(rotationInterface);
                rotationPanel.SetActive(true);
                telescope.SetImageAlpha(true);
                panZones[0].gameObject.SetActive(false);
                panZones[1].gameObject.SetActive(false);
            }
        }
        else if (telescopeDrag)
        {
            telescopeDrag = false;
            telescope.EndDrag();
        }
    }

    void FixedUpdate()
    {
        if (interactible && interactibleState == EInteractibleState.DRAGNDROP)
        {
            Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, interactibleScreenPos.z);
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos) + interactibleOffset;

            // Check desk borders before moving
            if (mouseWorldPos.x >= deskMinX && mouseWorldPos.x <= deskMaxX && mouseWorldPos.z >= deskMinZ && mouseWorldPos.z <= deskMaxZ)
                interactible.MoveTo(mouseWorldPos);
        }
    }

    void StopNavigation()
    {
        navigation = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
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
