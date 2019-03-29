using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    Rigidbody rb;
    Vector3 rbPos;
    Vector3 targetPos;
    bool moving;
    bool waiting;
    float waitTime;
    float currentTime;

    void Start()
    {
        targetPos = new Vector3(3.25f, 1.95f, -1.2f);
        moving = false;
        waiting = false;
        waitTime = 0.7f;
        currentTime = 0;
    }

    public void AddToInventory(GameObject prefab)
    {
        GameObject newObject = Instantiate(prefab, transform);
        newObject.transform.position = new Vector3(3.25f, 1.95f, 0);
        rb = newObject.GetComponent<Rigidbody>();
        rbPos = newObject.transform.position;
        rb.useGravity = false;
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
            rbPos.z -= 0.1f;
            rb.position = rbPos;

            if (rb.position == targetPos)
            {
                rb.useGravity = true;
                moving = false;
                waiting = true;
            }
        }
    }
}
