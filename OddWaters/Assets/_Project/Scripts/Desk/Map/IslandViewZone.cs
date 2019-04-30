using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandViewZone : MonoBehaviour
{
    Island parentIsland;

    void Start()
    {
        parentIsland = transform.parent.GetComponent<Island>();    
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boat"))
            other.transform.GetComponent<Boat>().IslandInSight(parentIsland);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Boat"))
            other.transform.GetComponent<Boat>().IslandNoMoreInSight(parentIsland);
    }
}
