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

    [HideInInspector]
    public bool firstTimeVisiting = true;

    SpriteRenderer spriteRenderer;
    BoxCollider boxCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = visible;
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.enabled = visible;
    }

    public void Berth()
    {
        firstTimeVisiting = false;
    }

    public void Discover()
    {
        visible = true;
        spriteRenderer.enabled = true;
        boxCollider.enabled = true;
    }
}
