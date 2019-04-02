using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BlockInputEvent : GameEvent { public bool block; }

public class InputManager : MonoBehaviour
{
    [SerializeField]
    NavigationManager navigationManager;

    [SerializeField]
    ScreenManager screenManager;

    GameObject mouseProjection;

    // Desk

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
            if (Input.GetMouseButtonDown(0))
                HandleMouseLeftButtonDown();

            if (Input.GetMouseButtonUp(0))
                HandleMouseLeftButtonUp();

            if (Input.GetMouseButtonDown(1) && navigation)
                StopNavigation();

            if (!navigation && Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits = Physics.RaycastAll(ray);
                if (hits.Length > 0)
                {
                    if (hits.Any(hit => hit.collider.CompareTag("TelescopeCollider") || hit.collider.GetComponent<TelescopeElement>()))
                        telescope.Zoom(Input.GetAxis("Mouse ScrollWheel"));
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
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits.Length > 0)
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
                        grabbedObject = hitInfo.collider.GetComponent<Interactible>();
                        grabbedObject.Grab();
                        grabbedOjectScreenPos = mainCamera.WorldToScreenPoint(grabbedObject.gameObject.transform.position);
                        grabbedObjectOffset = grabbedObject.gameObject.transform.position - mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, grabbedOjectScreenPos.z));
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

    void HandleMouseLeftButtonUp()
    {
        if (grabbedObject)
        {
            grabbedObject.Drop();
            grabbedObject = null;
        }
        else if (telescopeDrag)
        {
            telescopeDrag = false;
            telescope.EndDrag();
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
