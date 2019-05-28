using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum EMoveType
{
    GIVE_FIRST_STEP,
    GIVE_SECOND_STEP,
    RECEIVE
}

public class Inventory : MonoBehaviour
{
    [SerializeField]
    InspectionInterface inspectionInterface;
    [SerializeField]
    TutorialManager tutorialManager;

    [SerializeField]
    Vector3 spawnPos = new Vector3(6.25f, 1.95f, 1);
    [SerializeField]
    Vector3 receiveEndPos = new Vector3(6.25f, 1.95f, -1.2f);
    Vector3 targetPos;
    [SerializeField]
    float drag = 7.37f;

    GameObject prefabToGive;
    GameObject newObject;
    BoxCollider boxCollider;
    Rigidbody rb;
    [SerializeField]
    GameObject previousObject;
   
    bool moving;
    EMoveType move;
    bool waiting;
    float waitTime;
    float currentTime;

    void Start()
    {
        moving = false;
        waiting = false;
        waitTime = 0.7f;
        currentTime = 0;

        if (previousObject)
        {
            boxCollider = previousObject.GetComponent<BoxCollider>();
            rb = previousObject.GetComponent<Rigidbody>();
        }
    }

    public bool TradeObjects(GameObject prefab)
    {
        prefabToGive = prefab;

        if (previousObject)
        {
            moving = true;
            move = EMoveType.GIVE_FIRST_STEP;

            boxCollider.isTrigger = true;
            rb.useGravity = false;
            rb.drag = 0;

            targetPos = previousObject.transform.position;
            targetPos.y += 1;
            rb.velocity = (targetPos - previousObject.transform.position);
            return true;
        }
        else
            AddObjectToInventory();

        return false;
    }

    void AddObjectToInventory()
    {
        moving = true;
        move = EMoveType.RECEIVE;

        newObject = Instantiate(prefabToGive, transform);
        previousObject = newObject;

        boxCollider = newObject.GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;

        rb = newObject.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 0;

        newObject.transform.localPosition = spawnPos;
        targetPos = receiveEndPos;
        rb.velocity = (targetPos - spawnPos);

        Interactible interactible = newObject.GetComponent<Interactible>();
        interactible.inspectionInterface = inspectionInterface;
        interactible.tutorialManager = tutorialManager;
    }

    void Update()
    {
        if (waiting)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= waitTime)
            {
                waiting = false;
                currentTime = 0;
                EventManager.Instance.Raise(new BlockInputEvent() { block = false, navigation = false });
            }
        }
        if (moving)
        {
            if (move == EMoveType.RECEIVE && newObject.transform.localPosition.z <= targetPos.z)
            {
                rb.velocity = Vector3.zero;
                rb.useGravity = true;
                rb.drag = drag;
                boxCollider.isTrigger = false;
                moving = false;
                waiting = true;
            }
            else if (move == EMoveType.GIVE_FIRST_STEP && previousObject.transform.localPosition.y >= targetPos.y)
            {
                move = EMoveType.GIVE_SECOND_STEP;

                targetPos = spawnPos;
                targetPos.x = previousObject.transform.localPosition.x;
                rb.velocity = (targetPos - previousObject.transform.localPosition);
            }
            else if (move == EMoveType.GIVE_SECOND_STEP && previousObject.transform.localPosition.z >= targetPos.z)
            {
                Destroy(previousObject);
                AddObjectToInventory();
            }
        }
    }
}
