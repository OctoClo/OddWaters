using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapElement : MonoBehaviour
{
    public string name;
    public bool visible;
    public Sprite elementSprite;
    public ELayer layer;

    [SerializeField]
    string discoverySound;

    [HideInInspector]
    public bool discovered = false;

    MeshRenderer meshRenderer;

    virtual protected void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = visible;
    }

    public IEnumerator Discover()
    {
        if (discoverySound.Length > 0)
        {
            AkSoundEngine.PostEvent(discoverySound, gameObject);
            yield return new WaitForSeconds(1.5f);
        }
        
        AkSoundEngine.PostEvent("Play_Note", gameObject);
        meshRenderer.enabled = true;
        visible = true;
        discovered = true;
    }
}
