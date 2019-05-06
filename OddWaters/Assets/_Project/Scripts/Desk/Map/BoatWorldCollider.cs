using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatInTyphoonEvent : GameEvent { };

public class BoatWorldCollider : MonoBehaviour
{
    Boat boat;

    void Start()
    {
        boat = transform.parent.gameObject.GetComponent<Boat>();    
    }

    void OnTriggerEnter(Collider other)
    {
        Island island = other.GetComponent<Island>();
        if (island)
        {
            boat.onAnIsland = true;
            boat.currentIsland = island;
        }
        else if (other.CompareTag("Typhoon"))
        {
            boat.inATyphoon = true;
            EventManager.Instance.Raise(new BoatInTyphoonEvent());
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
    }
}
