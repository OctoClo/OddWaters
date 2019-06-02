﻿using System.Collections;
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
    [Header("Trade Animations")]
    [SerializeField]
    Vector3 spawnPos = new Vector3(6.25f, 1.95f, 1);
    [SerializeField]
    Vector3 receiveEndPos = new Vector3(6.25f, 1.95f, -1.2f);
    Vector3 targetPos;
    [SerializeField]
    float drag = 7.37f;
    [SerializeField]
    GameObject previousObject;

    [Header("References")]
    [SerializeField]
    InspectionInterface inspectionInterface;
    [SerializeField]
    TutorialManager tutorialManager;

    GameObject prefabToGive;
    GameObject newObject;
    Collider[] colliders;
    Rigidbody rb;

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
            colliders = previousObject.GetComponents<Collider>();
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

            foreach (Collider collider in colliders)
                collider.isTrigger = true;
            rb.useGravity = false;
            rb.drag = 0;

            targetPos = previousObject.transform.position;
            targetPos.y += 1;
            rb.velocity = (targetPos - previousObject.transform.position);
        }
        else
            AddObjectToInventory();

        return (previousObject && prefabToGive);
    }

    void AddObjectToInventory()
    {
        if (prefabToGive)
        {
            moving = true;
            move = EMoveType.RECEIVE;

            newObject = Instantiate(prefabToGive, transform);
            previousObject = newObject;

            colliders = newObject.GetComponents<Collider>();
            foreach (Collider collider in colliders)
                collider.isTrigger = true;

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
        else
        {
            moving = false;
            waiting = false;
            currentTime = 0;
            EventManager.Instance.Raise(new BlockInputEvent() { block = false, navigation = false });
        }
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
                foreach (Collider collider in colliders)
                    collider.isTrigger = false;
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
