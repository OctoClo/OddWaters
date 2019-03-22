using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapZone : MonoBehaviour
{
    public int zoneNumber;
    public Texture telescopeTexture;

    [HideInInspector]
    public Map map;

    void OnTriggerEnter(Collider other)
    {
        map.currentZone = zoneNumber;
    }
}
