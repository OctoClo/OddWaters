using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    GameObject desk;
    float deskMinX;
    float deskMaxX;
    float deskMinZ;
    float deskMaxZ;
    
    Panorama panorama;
    Vector3 dragBeginPos;
    const float panoramaSpeedMultiplier = 0.001f;

    Camera mainCamera;
    Interactible grabbedObject;
    Vector3 grabbedOjectScreenPos;
    Vector3 grabbedObjectOffset;

    void Start()
    {
        Vector3 deskPosition = desk.transform.position;
        Vector3 deskScale = desk.transform.localScale;
        deskMinX = deskPosition.x - deskScale.x / 2;
        deskMaxX = deskPosition.x + deskScale.x / 2;
        deskMinZ = deskPosition.z - deskScale.z / 2;
        deskMaxZ = deskPosition.z + deskScale.z / 2;

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
                    panorama = hit.collider.GetComponent<Panorama>();
                    if (panorama)
                    {
                        dragBeginPos = Input.mousePosition;
                        Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y);
                        panorama.BeginDrag(mainCamera.ScreenToWorldPoint(mouseScreenPos));
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
            else if (panorama)
            {
                panorama.EndDrag();
                panorama = null;
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
        else if (panorama)
        {
            Vector3 dragCurrentPos = Input.mousePosition;
            panorama.UpdateSpeed(((dragCurrentPos - dragBeginPos) * panoramaSpeedMultiplier).x);
        }
    }
}
