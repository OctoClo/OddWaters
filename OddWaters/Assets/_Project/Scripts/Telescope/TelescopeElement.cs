using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaTyphoonActivatedEvent : GameEvent { public TelescopeElement element; }

public class TelescopeElement : MonoBehaviour
{
    [HideInInspector]
    public bool triggerActive = true;

    [HideInInspector]
    public bool needZoom = false;
    [HideInInspector]
    public bool needSight = false;
    [HideInInspector]
    public bool needSuperPrecision = false;
    [HideInInspector]
    public bool playClue = true;

    public bool inSight = false;
    [HideInInspector]
    public int startAngle;
    public int angleToBoat;
    [HideInInspector]
    public bool audio = false;

    [HideInInspector]
    public MapElement elementDiscover;

    [HideInInspector]
    public TelescopeElement cloneElement;

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
            if (playClue)
                AkSoundEngine.PostEvent("Play_Clue_" + name, gameObject);
            if (elementDiscover.name == "MegaTyphoon")
                EventManager.Instance.Raise(new MegaTyphoonActivatedEvent() { element = this });
        }
    }

    void Update()
    {
        if (audio)
            AkSoundEngine.SetRTPCValue("Angle", angleToBoat, gameObject);
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
