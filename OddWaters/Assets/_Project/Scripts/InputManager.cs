using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockInputEvent : GameEvent { public bool block; }

public class InputManager : MonoBehaviour
{
    [SerializeField]
    NavigationManager navigationManager;

    [SerializeField]
    ScreenManager screenManager;

    // Inventory

    [SerializeField]
    GameObject desk;
    float deskMinX;
    float deskMaxX;
    float deskMinZ;
    float deskMaxZ;

    Camera mainCamera;
    bool blockInput;

    Interactible grabbedObject;
    Vector3 grabbedOjectScreenPos;
    Vector3 grabbedObjectOffset;

    // Telescope

    [SerializeField]
    Telescope telescope;
    Vector3 mouseScreenPos;
    Vector3 dragBeginPos;
    TelescopeElement telescopeElement;

    bool telescopeClicked;
    float telescopeTimeSinceClick;
    float telescopeClickTime;

    // Map

    [SerializeField]
    [Tooltip("Dans l'ordre : mer, île, KO")]
    Sprite[] cursorsNavigation;
    Vector2 cursorOffset;
    bool navigation;
    
    void Start()
    {
        Vector3 position = desk.transform.position;
        Vector3 scale = desk.transform.localScale;
        deskMinX = position.x - scale.x / 2;
        deskMaxX = position.x + scale.x / 2;
        deskMinZ = position.z - scale.z / 2;
        deskMaxZ = position.z + scale.z / 2;

        mainCamera = Camera.main;
        blockInput = false;

        telescopeClicked = false;
        telescopeTimeSinceClick = 0;
        telescopeClickTime = 0.2f;

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
            if (Input.GetMouseButtonDown(0))
                HandleMouseLeftButtonDown();

            if (Input.GetMouseButtonUp(0))
                HandleMouseLeftButtonUp();

            if (Input.GetMouseButtonDown(1) && navigation)
                StopNavigation();

            if (!navigation && Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.GetComponent<Telescope>() || hit.collider.GetComponent<TelescopeElement>())
                        telescope.Zoom(Input.GetAxis("Mouse ScrollWheel"));
                }
            }
        }

        if (telescopeClicked)
        {
            telescopeTimeSinceClick += Time.deltaTime;

            if (telescopeTimeSinceClick >= telescopeClickTime)
            {
                telescope.BeginDrag(mainCamera.ScreenToWorldPoint(mouseScreenPos));
                Vector3 dragCurrentPos = Input.mousePosition;
                telescope.UpdateSpeed(((dragCurrentPos - dragBeginPos)).x);
            }
        }

        if (navigation)
        {
            mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y);
            int cursorType = (int)navigationManager.GetNavigationResult(mainCamera.ScreenToWorldPoint(mouseScreenPos));
            Cursor.SetCursor(cursorsNavigation[cursorType].texture, cursorOffset, CursorMode.Auto);
        }
    }

    void HandleMouseLeftButtonDown()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (navigation)
            {
                Island island = hit.collider.GetComponent<Island>();
                if (island) // Island navigation
                {
                    StopNavigation();
                    navigationManager.NavigateToIsland(island);
                }
                else
                {
                    MapZone mapZone = hit.collider.transform.GetComponentInParent<MapZone>();
                    if (mapZone) // Sea navigation
                    {
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
                if (hit.collider.CompareTag("Boat")) // Navigation
                {
                    navigation = true;
                }
                else
                {
                    grabbedObject = hit.collider.GetComponent<Interactible>();
                    if (grabbedObject) // Inventory
                    {
                        grabbedObject.Grab();
                        grabbedOjectScreenPos = mainCamera.WorldToScreenPoint(grabbedObject.gameObject.transform.position);
                        grabbedObjectOffset = grabbedObject.gameObject.transform.position - mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, grabbedOjectScreenPos.z));
                    }
                    else
                    {
                        telescopeElement = hit.collider.GetComponent<TelescopeElement>();
                        Telescope telescopeHit = hit.collider.GetComponent<Telescope>();
                        if (!navigation && (telescopeElement || telescopeHit)) // Telescope
                        {
                            telescopeClicked = true;
                            dragBeginPos = Input.mousePosition;
                            mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y);
                        }
                    }
                }
                
            }
        }
    }

    void HandleMouseLeftButtonUp()
    {
        if (grabbedObject)
        {
            grabbedObject.Drop();
            grabbedObject = null;
        }
        else if (telescopeClicked)
        {
            if (telescopeTimeSinceClick <= telescopeClickTime && telescopeElement)
                telescopeElement.Trigger();
            else
                telescope.EndDrag();

            telescopeClicked = false;
            telescopeTimeSinceClick = 0;
        }
    }

    void FixedUpdate()
    {
        if (grabbedObject)
        {
            Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, grabbedOjectScreenPos.z);
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos) + grabbedObjectOffset;

            // Check desk borders before moving
            if (mouseWorldPos.x >= deskMinX && mouseWorldPos.x <= deskMaxX && mouseWorldPos.z >= deskMinZ && mouseWorldPos.z <= deskMaxZ)
                grabbedObject.MoveTo(mouseWorldPos);
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
}
