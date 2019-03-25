using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    NavigationManager navigationManager;

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

    Telescope telescope;
    Vector3 dragBeginPos;

    void Start()
    {
        Vector3 position = desk.transform.position;
        Vector3 scale = desk.transform.localScale;
        deskMinX = position.x - scale.x / 2;
        deskMaxX = position.x + scale.x / 2;
        deskMinZ = position.z - scale.z / 2;
        deskMaxZ = position.z + scale.z / 2;

        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                grabbedObject = hit.collider.GetComponent<Interactible>();
                if (grabbedObject)
                {
                    grabbedObject.Grab();
                    grabbedOjectScreenPos = mainCamera.WorldToScreenPoint(grabbedObject.gameObject.transform.position);
                    grabbedObjectOffset = grabbedObject.gameObject.transform.position - mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, grabbedOjectScreenPos.z));
                }
                else
                {
                    MapZone mapZone = hit.collider.transform.GetComponentInParent<MapZone>();
                    if (mapZone)
                    {
                        if (mapZone.visible)
                        {
                            Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y);
                            navigationManager.NavigateTo(mainCamera.ScreenToWorldPoint(mouseScreenPos));
                        }
                        else
                        {
                            Debug.Log("Not yet...");
                        }
                    }
                    else
                    {
                        telescope = hit.collider.GetComponent<Telescope>();
                        if (telescope)
                        {
                            dragBeginPos = Input.mousePosition;
                            Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y);
                            telescope.BeginDrag(mainCamera.ScreenToWorldPoint(mouseScreenPos));
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
            else if (telescope)
            {
                telescope.EndDrag();
                telescope = null;
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
        else if (telescope)
        {
            Vector3 dragCurrentPos = Input.mousePosition;
            telescope.UpdateSpeed(((dragCurrentPos - dragBeginPos)).x);
        }
    }
}
