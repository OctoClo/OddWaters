using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaTyphoonActivatedEvent : GameEvent { public TelescopeElement element; }

public class TelescopeElement : MonoBehaviour
{
    // General
    [HideInInspector]
    public bool triggerActive = true;
    [HideInInspector]
    public MapElement elementDiscover;
    [HideInInspector]
    public TelescopeElement cloneElement;

    // Discovery
    [HideInInspector]
    public bool needZoom = false;
    [HideInInspector]
    public bool needSight = false;
    [HideInInspector]
    public bool needSuperPrecision = false;

    // Audio
    [HideInInspector]
    public bool audio = false;
    GameObject audioPlayer;
    [HideInInspector]
    public bool playClue = true;
    
    // Angle and in sight
    [HideInInspector]
    public int startAngle;
    public int angleToBoat;
    public bool inSight = false;

    public void Trigger(bool tutorial, TutorialManager tutorialManager)
    {
        triggerActive = false;
        cloneElement.triggerActive = false;
        StartCoroutine(elementDiscover.Discover(tutorial, tutorialManager));
    }

    void Start()
    {
        if (audio)
        {
            audioPlayer = elementDiscover.name.Equals("MegaTyphoon") ? CursorManager.Instance.gameObject : gameObject;

            if (playClue)
            {
                AkSoundEngine.PostEvent("Play_Clue_" + name, audioPlayer);
                Debug.Log("Playing clue for " + name);
            }

            if (elementDiscover.name.Equals("MegaTyphoon"))
                EventManager.Instance.Raise(new MegaTyphoonActivatedEvent() { element = this });
        }
    }

    void Update()
    {
        if (audio)
            AkSoundEngine.SetRTPCValue("Angle", angleToBoat, audioPlayer);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TelescopeCollider"))
            inSight = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TelescopeCollider"))
            inSight = false;
    }
}
