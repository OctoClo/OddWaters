using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatInTyphoonEvent : GameEvent { };

public class Boat : MonoBehaviour
{
    List<Island> islandsInSight;

    void Start()
    {
        islandsInSight = new List<Island>();   
    }

    public void IslandInSight(Island island)
    {
        islandsInSight.Add(island);
    }

    public void IslandNoMoreInSight(Island island)
    {
        islandsInSight.Remove(island);
    }

    public List<Island> GetIslandsInSight()
    {
        return islandsInSight;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Typhoon"))
            EventManager.Instance.Raise(new BoatInTyphoonEvent());
    }
}
