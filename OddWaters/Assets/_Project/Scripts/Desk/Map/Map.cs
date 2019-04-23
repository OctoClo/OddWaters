﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField]
    GameObject zonesFolder;
    public MapZone[] mapZones;
    
    public Island[] islands;

    [HideInInspector]
    public int currentZone;

    void Start()
    {
        mapZones = new MapZone[5];
        islands = new Island[4];

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
                islands[islandCounter] = island;
                islandCounter++;
            }
        }

        Debug.Log(mapZones);
        Debug.Log(islands);
    }

    void OnEnable()
    {
        EventManager.Instance.AddListener<DiscoverZoneEvent>(OnDiscoverZoneEvent);
        EventManager.Instance.AddListener<DiscoverIslandEvent>(OnDiscoverIslandEvent);
    }

    void OnDisable()
    {
        EventManager.Instance.RemoveListener<DiscoverZoneEvent>(OnDiscoverZoneEvent);
        EventManager.Instance.RemoveListener<DiscoverIslandEvent>(OnDiscoverIslandEvent);
    }

    public Sprite GetCurrentZoneSprite()
    {
        return mapZones[currentZone].telescopeSprite;
    }

    void OnDiscoverZoneEvent(DiscoverZoneEvent e)
    {
        Debug.Log("Discovered zone n°" + e.zoneNumber);
        mapZones[e.zoneNumber].Discover();
    }

    void OnDiscoverIslandEvent(DiscoverIslandEvent e)
    {
        Island island = islands[e.islandNumber];
        if (!island.visible)
        {
            Debug.Log("Discovered island n°" + e.islandNumber);
            island.visible = true;
            StartCoroutine(islands[e.islandNumber].Discover());
        }
        
    }
}
