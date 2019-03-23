using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapZone : MonoBehaviour
{
    public int zoneNumber;
    public Texture telescopeTexture;

    public bool visible;
    [SerializeField]
    Sprite visibleSprite;
    [SerializeField]
    Sprite invisibleSprite;

    [HideInInspector]
    public Map map;

    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = (visible ? visibleSprite : invisibleSprite);
    }

    void OnTriggerEnter(Collider other)
    {
        map.currentZone = zoneNumber;
    }
}
