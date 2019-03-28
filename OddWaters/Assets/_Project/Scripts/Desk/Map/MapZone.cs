using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapZoneChangeEvent : GameEvent { public int currentZone; }

public class MapZone : MonoBehaviour
{
    public int zoneNumber;
    public Sprite telescopeSprite;
    
    public bool visible;

    [SerializeField]
    Sprite visibleSprite;
    [SerializeField]
    Sprite invisibleSprite;

    SpriteRenderer spriteRenderer;
    int childCount;

    void Start()
    {
        childCount = transform.childCount;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = (visible ? visibleSprite : invisibleSprite);
        ActivateIslands();
    }

    public void Discover()
    {
        visible = true;
        spriteRenderer.sprite = visibleSprite;
        ActivateIslands();
    }

    void ActivateIslands()
    {
        for (int i = 0; i < childCount; i++)
            transform.GetChild(i).gameObject.SetActive(visible);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Boat"))
            EventManager.Instance.Raise(new MapZoneChangeEvent() { currentZone = zoneNumber });
    }
}
