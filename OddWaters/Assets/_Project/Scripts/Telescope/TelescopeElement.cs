using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelescopeElement : MonoBehaviour
{
    [HideInInspector]
    public bool triggerActive = true;

    [HideInInspector]
    public bool needZoom = false;

    public bool inSight = false;
    [HideInInspector]
    public int startAngle;
    public int angleToBoat;

    [HideInInspector]
    public MapElement elementDiscover;

    [HideInInspector]
    public TelescopeElement cloneElement;

    public void Trigger(bool tutorial, TutorialManager tutorialManager)
    {
        triggerActive = false;
        cloneElement.triggerActive = false;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
        cloneElement.gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
        StartCoroutine(elementDiscover.Discover(tutorial, tutorialManager));
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
