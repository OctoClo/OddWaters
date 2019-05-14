using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoverZoneEvent : GameEvent { public int zoneNumber; }

public class Island : MapElement
{
    public int islandNumber;
    public int nextZone;
    public Sprite background;
    public Sprite character;
    public GameObject objectToGive;

    [HideInInspector]
    public bool firstTimeVisiting = true;

    public void Berth()
    {
        AkSoundEngine.PostEvent("Play_AMB_" + name, gameObject);
        firstTimeVisiting = false;
    }
}
