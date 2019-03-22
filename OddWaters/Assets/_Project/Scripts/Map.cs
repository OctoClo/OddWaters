using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField]
    GameObject boat;
    float boatPosY;

    [SerializeField]
    GameObject mapImage;
    MapZone[] mapZones;
    int nbZones;
    [HideInInspector]
    public int currentZone;

    void Start()
    {
        boatPosY = boat.transform.position.y;

        nbZones = mapImage.transform.childCount;
        mapZones = new MapZone[nbZones];
        for (int i = 0; i < nbZones; i++)
        {
            mapZones[i] = mapImage.transform.GetChild(i).GetComponent<MapZone>();
            mapZones[i].map = this;
        }
    }

    public void MoveTo(Vector3 newPosition)
    {
        if ((newPosition - boat.transform.position).magnitude >= 0.5f)
        {
            newPosition.y = boatPosY;
            boat.transform.position = newPosition;
        }
    }

    public Texture GetNewZoneTexture()
    {
        return mapZones[currentZone].telescopeTexture;
    }
}
