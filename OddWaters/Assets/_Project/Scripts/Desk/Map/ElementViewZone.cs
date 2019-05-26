using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementViewZone : MonoBehaviour
{
    [HideInInspector]
    public int elementZone;
    MapElement element;

    void Start()
    {
        element = transform.parent.GetComponent<MapElement>();
        elementZone = element.transform.parent.GetComponent<MapZone>().zoneNumber;
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
