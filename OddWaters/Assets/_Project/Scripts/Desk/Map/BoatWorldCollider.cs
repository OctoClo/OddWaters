using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatInTyphoonEvent : GameEvent { public GameObject typhoon; };
public class BoatInMapElement : GameEvent { public bool exit; public ElementViewZone elementZone; }

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
            EventManager.Instance.Raise(new BoatInTyphoonEvent() { typhoon = other.gameObject });
        }
        else
        {
            ElementViewZone viewZone = other.GetComponent<ElementViewZone>();
            if (viewZone && !viewZone.transform.parent.gameObject.GetComponent<Island>())
                EventManager.Instance.Raise(new BoatInMapElement() { exit = false, elementZone = viewZone });
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
        else
        {
            ElementViewZone viewZone = other.GetComponent<ElementViewZone>();
            if (viewZone && !viewZone.transform.parent.gameObject.GetComponent<Island>())
                EventManager.Instance.Raise(new BoatInMapElement() { exit = true, elementZone = viewZone });
        }
    }
}
