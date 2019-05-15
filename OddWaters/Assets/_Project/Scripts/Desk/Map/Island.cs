using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoverZoneEvent : GameEvent { public int zoneNumber; }

public class Island : MapElement
{
    public int islandNumber;
    public int nextZone;
    public Sprite background;
    public Sprite character;
    public GameObject objectToGive;
    public TextAsset speech;

    [HideInInspector]
    public Dialogue dialogue;

    [HideInInspector]
    public bool firstTimeVisiting = true;

    override protected void Start()
    {
        base.Start();
        dialogue = JsonUtility.FromJson<Dialogue>(speech.text);
        firstTimeVisiting = true;
    }

    public void Berth()
    {
        AkSoundEngine.PostEvent("Play_AMB_" + name, gameObject);
        firstTimeVisiting = false;
    }
}
