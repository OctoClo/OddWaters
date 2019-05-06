using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapElement : MonoBehaviour
{
    public string name;
    public bool visible;
    public Sprite elementSprite;
    public ELayer layer;

    [HideInInspector]
    public bool discovered = false;

    MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = visible;
    }

    public IEnumerator Discover()
    {
        AkSoundEngine.PostEvent("Play_Discovery_Acte1", gameObject);
        yield return new WaitForSeconds(1.5f);
        AkSoundEngine.PostEvent("Play_Note", gameObject);
        meshRenderer.enabled = true;
        visible = true;
        discovered = true;
    }
}
