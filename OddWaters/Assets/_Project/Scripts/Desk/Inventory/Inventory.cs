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

    Rigidbody rb;
    Vector3 rbPos;
   
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
        GameObject newObject = Instantiate(prefab, transform);
        newObject.transform.position = spawnPos;
        rb = newObject.GetComponent<Rigidbody>();
        rbPos = newObject.transform.position;
        rb.useGravity = false;
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
            rbPos.z -= 0.03f;
            rb.position = rbPos;

            if ((rb.position - targetPos).sqrMagnitude <= 0.01f)
            {
                rb.useGravity = true;
                moving = false;
                waiting = true;
            }
        }
    }
}
