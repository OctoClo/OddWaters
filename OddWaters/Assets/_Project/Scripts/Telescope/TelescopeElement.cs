using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoverIslandEvent : GameEvent { public int islandNumber; }

public class TelescopeElement : MonoBehaviour
{
    [HideInInspector]
    public int islandDiscoverNumber;

    [HideInInspector]
    public Island islandDiscover;

    [HideInInspector]
    public GameObject cloneElement;

    public void Trigger()
    {
        cloneElement.GetComponent<TelescopeElement>().enabled = false;
        EventManager.Instance.Raise(new DiscoverIslandEvent() { islandNumber = islandDiscoverNumber });
    }
}
