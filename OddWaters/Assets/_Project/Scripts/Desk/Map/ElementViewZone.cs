using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementViewZone : MonoBehaviour
{
    [HideInInspector]
    public int zone;
    MapElement element;

    void Start()
    {
        element = transform.parent.GetComponent<MapElement>();
        zone = element.transform.parent.GetComponent<MapZone>().zoneNumber;
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
