using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    InspectionInterface inspectionInterface;

    [SerializeField]
    Vector3 spawnPos = new Vector3(6.25f, 1.95f, 1);
    [SerializeField]
    Vector3 targetPos = new Vector3(6.25f, 1.95f, -1.2f);

    GameObject newObject;
    BoxCollider boxCollider;
    Rigidbody rb;
    Vector3 currentPos;
   
    bool moving;
    bool waiting;
    float waitTime;
    float currentTime;

    void Start()
    {
        moving = false;
        waiting = false;
        waitTime = 0.7f;
        currentTime = 0;
    }

    public void AddToInventory(GameObject prefab)
    {
        newObject = Instantiate(prefab, transform);

        boxCollider = newObject.GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;

        rb = newObject.GetComponent<Rigidbody>();
        rb.useGravity = false;

        newObject.transform.localPosition = spawnPos;
        currentPos = spawnPos;

        Interactible interactible = newObject.GetComponent<Interactible>();
        interactible.inspectionInterface = inspectionInterface;

        moving = true;
    }

    void Update()
    {
        if (waiting)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= waitTime)
            {
                waiting = false;
                EventManager.Instance.Raise(new BlockInputEvent() { block = false });
            }
        }
        if (moving)
        {
            currentPos.z -= 0.03f;
            newObject.transform.localPosition = currentPos;

            if ((currentPos - targetPos).sqrMagnitude <= 0.01f)
            {
                rb.useGravity = true;
                boxCollider.isTrigger = false;
                moving = false;
                waiting = true;
            }
        }
    }
}
