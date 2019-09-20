using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatWorldCollider : MonoBehaviour
{
    Boat boat;
    List<GameObject> safeZones;

    void Start()
    {
        boat = transform.parent.gameObject.GetComponent<Boat>();
        safeZones = new List<GameObject>();
    }

    void OnTriggerEnter(Collider other)
    {
        Island island = other.GetComponent<Island>();
        if (island)
        {
            boat.onAnIsland = true;
            boat.currentIsland = island;
        }
        else if (other.CompareTag("SafeZone"))
        {
            safeZones.Add(other.gameObject);
            if (safeZones.Count == 1)
                boat.safeZone = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Typhoon"))
        {
            boat.inATyphoon = !boat.safeZone;

            if (!boat.safeZone)
                boat.typhoon = other.gameObject;
            else
                other.GetComponent<Renderer>().enabled = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Island island = other.GetComponent<Island>();
        if (island)
        {
            boat.onAnIsland = false;
            boat.currentIsland = null;
        }
        else if (other.CompareTag("Typhoon"))
            boat.inATyphoon = false;
        else if (other.CompareTag("SafeZone"))
        {
            safeZones.Remove(other.gameObject);
            if (safeZones.Count == 0)
                boat.safeZone = false;
        }
    }
}
