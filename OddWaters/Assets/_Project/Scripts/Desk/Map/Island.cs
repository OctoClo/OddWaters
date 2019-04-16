using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoverZoneEvent : GameEvent { public int zoneNumber; }

public class Island : MonoBehaviour
{
    public int islandNumber;
    public int nextZone;
    public bool visible;
    public Sprite illustration;
    public Sprite character;
    public GameObject objectToGive;
    public GameObject island3D;

    [HideInInspector]
    public bool firstTimeVisiting = true;

    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = visible;
    }

    public void Berth()
    {
        AkSoundEngine.PostEvent("Play_AMB_Island" + islandNumber, gameObject);
        firstTimeVisiting = false;
    }

    public void Discover()
    {
        visible = true;
        spriteRenderer.enabled = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ViewZone"))
        {
            other.transform.parent.GetComponent<Boat>().IslandInSight(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ViewZone"))
        {
            other.transform.parent.GetComponent<Boat>().IslandNoMoreInSight(this);
        }
    }
}
