using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementViewZone : MonoBehaviour
{
    MapElement element;

    void Start()
    {
        element = transform.parent.GetComponent<MapElement>();    
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boat"))
            other.transform.GetComponent<Boat>().ElementInSight(element);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Boat"))
            other.transform.GetComponent<Boat>().ElementNoMoreInSight(element);
    }
}
