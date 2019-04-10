using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoverIslandEvent : GameEvent { public int islandNumber; }

public class TelescopeElement : MonoBehaviour
{
    public int islandDiscoverNumber;

    [HideInInspector]
    public GameObject cloneElement;

    public void Trigger()
    {
        EventManager.Instance.Raise(new DiscoverIslandEvent() { islandNumber = islandDiscoverNumber });
        cloneElement.gameObject.SetActive(false);
        gameObject.gameObject.SetActive(false);
    }
}
