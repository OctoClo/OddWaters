using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField]
    GameObject zonesFolder;
    MapZone[] mapZones;
    
    Island[] elements;

    [HideInInspector]
    public int currentZone;

    void Start()
    {
        mapZones = new MapZone[5];
        elements = new Island[4];

        int mapCounter = 0;
        int islandCounter = 0;

        Transform[] allChildren = transform.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            MapZone mapZone = child.gameObject.GetComponent<MapZone>();
            Island island = child.gameObject.GetComponent<Island>();
            if (mapZone)
            {
                mapZones[mapCounter] = mapZone;
                mapCounter++;
            }
            else if (island)
            {
                elements[islandCounter] = island;
                islandCounter++;
            }
        }

        ChangeZone(0);
    }

    void OnEnable()
    {
        EventManager.Instance.AddListener<DiscoverZoneEvent>(OnDiscoverZoneEvent);
    }

    void OnDisable()
    {
        EventManager.Instance.RemoveListener<DiscoverZoneEvent>(OnDiscoverZoneEvent);
    }

    public void ChangeZone(int newZone)
    {
        currentZone = newZone;
        mapZones[currentZone].seaIntensity.SetValue();
        mapZones[currentZone].weather.SetValue();
    }

    public GameObject GetCurrentPanorama()
    {
        return mapZones[currentZone].GetPanorama();
    }

    void OnDiscoverZoneEvent(DiscoverZoneEvent e)
    {
        Debug.Log("Discovered zone n°" + e.zoneNumber);
        mapZones[e.zoneNumber].Discover();
    }
}
