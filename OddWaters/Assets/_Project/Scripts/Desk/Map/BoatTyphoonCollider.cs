using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatInTyphoonEvent : GameEvent { };

public class BoatTyphoonCollider : MonoBehaviour
{
    Boat boat;

    void Start()
    {
        boat = transform.parent.gameObject.GetComponent<Boat>();    
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Typhoon"))
        {
            boat.inATyphoon = true;
            EventManager.Instance.Raise(new BoatInTyphoonEvent());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Typhoon"))
            boat.inATyphoon = false;
    }
}
