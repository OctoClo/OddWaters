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
    public Sprite islandSprite;

    [HideInInspector]
    public bool firstTimeVisiting = true;

    MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = visible;
    }

    public void Berth()
    {
        AkSoundEngine.PostEvent("Play_AMB_Island" + islandNumber, gameObject);
        firstTimeVisiting = false;
    }

    public IEnumerator Discover()
    {
        AkSoundEngine.PostEvent("Play_Discovery_Acte1", gameObject);
        yield return new WaitForSeconds(1.5f);
        AkSoundEngine.PostEvent("Play_Note", gameObject);
        meshRenderer.enabled = true;
    }
}
