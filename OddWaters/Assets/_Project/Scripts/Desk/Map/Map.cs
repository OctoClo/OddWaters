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
            mapZones[i] = zonesFolder.transform.GetChild(i).GetComponent<MapZone>();
    }

    void OnEnable()
    {
        EventManager.Instance.AddListener<DiscoverZoneEvent>(OnDiscoverZoneEvent);
        EventManager.Instance.AddListener<MapZoneChangeEvent>(OnMapZoneChangeEvent);
    }

    void OnDisable()
    {
        EventManager.Instance.RemoveListener<DiscoverZoneEvent>(OnDiscoverZoneEvent);
        EventManager.Instance.RemoveListener<MapZoneChangeEvent>(OnMapZoneChangeEvent);
    }

    public Sprite GetCurrentZoneSprite()
    {
        return mapZones[currentZone].telescopeSprite;
    }

    void OnMapZoneChangeEvent(MapZoneChangeEvent e)
    {
        currentZone = e.currentZone;
    }

    void OnDiscoverZoneEvent(DiscoverZoneEvent e)
    {
        Debug.Log("Discovered zone n°" + e.zoneNumber);
        mapZones[e.zoneNumber].visible = true;
    }
}
