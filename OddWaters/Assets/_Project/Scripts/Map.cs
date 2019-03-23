using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField]
    GameObject zonesFolder;
    MapZone[] mapZones;
    int nbZones;

    [HideInInspector]
    public int currentZone;

    void Start()
    {
        nbZones = zonesFolder.transform.childCount;
        mapZones = new MapZone[nbZones];
        for (int i = 0; i < nbZones; i++)
        {
            mapZones[i] = zonesFolder.transform.GetChild(i).GetComponent<MapZone>();
            mapZones[i].map = this;
        }
    }

    public Texture GetCurrentZoneTexture()
    {
        return mapZones[currentZone].telescopeTexture;
    }
}
