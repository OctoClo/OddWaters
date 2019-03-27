using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    NavigationManager navigationManager;

    [SerializeField]
    ScreenManager screenManager;

    [SerializeField]
    GameObject desk;
    float deskMinX;
    float deskMaxX;
    float deskMinZ;
    float deskMaxZ;
    
    Camera mainCamera;

    Interactible grabbedObject;
    Vector3 grabbedOjectScreenPos;
    Vector3 grabbedObjectOffset;

    [SerializeField]
    Telescope telescope;
    Vector3 mouseScreenPos;
    Vector3 dragBeginPos;
    TelescopeElement telescopeElement;

    bool telescopeClicked;
    float telescopeTimeSinceClick;
    float telescopeClickTime;

    void Start()
    {
        Vector3 position = desk.transform.position;
        Vector3 scale = desk.transform.localScale;
        deskMinX = position.x - scale.x / 2;
        deskMaxX = position.x + scale.x / 2;
        deskMinZ = position.z - scale.z / 2;
        deskMaxZ = position.z + scale.z / 2;

        mainCamera = Camera.main;

        telescopeClicked = false;
        telescopeTimeSinceClick = 0;
        telescopeClickTime = 0.2f;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                grabbedObject = hit.collider.GetComponent<Interactible>();
                if (grabbedObject) // Interactibles drag
                {
                    grabbedObject.Grab();
                    grabbedOjectScreenPos = mainCamera.WorldToScreenPoint(grabbedObject.gameObject.transform.position);
                    grabbedObjectOffset = grabbedObject.gameObject.transform.position - mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, grabbedOjectScreenPos.z));
                }
                else
                {
                    Island island = hit.collider.GetComponent<Island>();
                    if (island)
                    {
                        screenManager.Berth(island.illustration, island.character, island.firstTimeVisiting, island.objectToGive);
                        island.Berth();
                    }
                    else
                    {
                        MapZone mapZone = hit.collider.transform.GetComponentInParent<MapZone>();
                        if (mapZone) // Map navigation
                        {
                            if (mapZone.visible)
                            {
                                Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y);
                                navigationManager.NavigateTo(mainCamera.ScreenToWorldPoint(mouseScreenPos));

                                if (screenManager.screenType == EScreenType.ISLAND_MIXED)
                                    screenManager.LeaveIsland();
                            }
                            else
                            {
                                Debug.Log("Zone not yet available");
                            }
                        }
                        else
                        {
                            telescopeElement = hit.collider.GetComponent<TelescopeElement>();
                            Telescope telescopeHit = hit.collider.GetComponent<Telescope>();
                            if (telescopeElement || telescopeHit) // Telescope
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

        if (Input.GetMouseButtonUp(0))
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

        if (telescopeClicked)
        {
            telescopeTimeSinceClick += Time.deltaTime;

            if (telescopeTimeSinceClick == telescopeClickTime)
                telescope.BeginDrag(mainCamera.ScreenToWorldPoint(mouseScreenPos));

            if (telescopeTimeSinceClick >= telescopeClickTime)
            {
                Vector3 dragCurrentPos = Input.mousePosition;
                telescope.UpdateSpeed(((dragCurrentPos - dragBeginPos)).x);
            }
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
}
