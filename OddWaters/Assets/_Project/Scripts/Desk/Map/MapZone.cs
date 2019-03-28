using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapZoneChangeEvent : GameEvent { public int currentZone; }

public class MapZone : MonoBehaviour
{
    public int zoneNumber;
    public Sprite telescopeSprite;

    public bool visible
    {
        get
        {
            return _visible;
        }
        set
        {
            _visible = value;
            spriteRenderer.sprite = (_visible ? visibleSprite : invisibleSprite);
        }
    }
    [SerializeField]
    bool _visible;

    [SerializeField]
    Sprite visibleSprite;
    [SerializeField]
    Sprite invisibleSprite;

    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = (visible ? visibleSprite : invisibleSprite);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Boat")
            EventManager.Instance.Raise(new MapZoneChangeEvent() { currentZone = zoneNumber });
    }
}
