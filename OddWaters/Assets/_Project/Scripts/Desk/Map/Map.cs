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

    [HideInInspector]
    public GameObject currentPanorama;

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

        currentZone = 0;
        currentPanorama = mapZones[0].telescopePanorama;
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
        currentPanorama = mapZones[currentZone].telescopePanorama;
    }

    public GameObject GetCurrentPanorama()
    {
        if (currentPanorama)
        {
            GameObject panorama = currentPanorama;
            currentPanorama = null;
            return panorama;
        }

        return null;
    }

    void OnDiscoverZoneEvent(DiscoverZoneEvent e)
    {
        Debug.Log("Discovered zone n°" + e.zoneNumber);
        mapZones[e.zoneNumber].Discover();
    }
}
