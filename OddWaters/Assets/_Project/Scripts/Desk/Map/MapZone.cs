using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = (visible ? visibleSprite : invisibleSprite);
    }

    public void Discover()
    {
        visible = true;
        spriteRenderer.sprite = visibleSprite;
    }
}
