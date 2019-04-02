using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoverZoneEvent : GameEvent { public int zoneNumber; }

public class TelescopeElement : MonoBehaviour
{
    public int zoneDiscoverNumber;

    [HideInInspector]
    public GameObject cloneElement;

    public void Trigger()
    {
        EventManager.Instance.Raise(new DiscoverZoneEvent() { zoneNumber = zoneDiscoverNumber });
        cloneElement.gameObject.SetActive(false);
        gameObject.gameObject.SetActive(false);
    }
}
