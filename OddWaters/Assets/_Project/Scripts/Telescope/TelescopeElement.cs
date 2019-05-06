using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelescopeElement : MonoBehaviour
{
    [HideInInspector]
    public bool triggerActive = true;

    [HideInInspector]
    public bool needZoom = false;

    [HideInInspector]
    public MapElement elementDiscover;

    [HideInInspector]
    public TelescopeElement cloneElement;

    public void Trigger()
    {
        triggerActive = false;
        cloneElement.triggerActive = false;
        StartCoroutine(elementDiscover.Discover());
    }
}
