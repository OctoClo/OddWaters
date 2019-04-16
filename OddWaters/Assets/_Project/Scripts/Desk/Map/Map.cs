using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField]
    GameObject zonesFolder;
    MapZone[] mapZones;
    int nbZones;

    [SerializeField]
    Island[] islands;

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
        Debug.Log("Discovered island n°" + e.islandNumber);
        islands[e.islandNumber].Discover();
        AkSoundEngine.PostEvent("Play_Note",gameObject);
        AkSoundEngine.PostEvent("Play_Discovery_Acte1",gameObject);
    }
}
