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
        if (Input.GetMouseButtonUp(0))
        {
            if (grabbedObject)
            {
                grabbedObject = null;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            GrabObject();
        }        
    }

    void GrabObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 20.0f))
        {
            Interactible interactible = hit.collider.GetComponent<Interactible>();
            if (interactible)
            {
                grabbedObject = interactible;
                grabbedOjectScreenPos = mainCamera.WorldToScreenPoint(grabbedObject.gameObject.transform.position);
                grabbedObjectOffset = grabbedObject.gameObject.transform.position - mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, grabbedOjectScreenPos.z));
            }
        }
    }

    void FixedUpdate()
    {
        if (grabbedObject)
        {
            Vector3 mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, grabbedOjectScreenPos.z);
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos) + grabbedObjectOffset;
            if (mouseWorldPos.x >= deskMinX && mouseWorldPos.x <= deskMaxX && mouseWorldPos.z >= deskMinZ && mouseWorldPos.z <= deskMaxZ)
                grabbedObject.MoveTo(mouseWorldPos);
        }
    }
}
