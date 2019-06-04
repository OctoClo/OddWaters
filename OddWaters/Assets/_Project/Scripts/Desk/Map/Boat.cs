using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatInMapElementEvent : GameEvent { public bool exit; public ElementViewZone elementZone; }
public class BoatAsksTelescopeRefreshEvent : GameEvent { public MapElement element; }

public class Boat : MonoBehaviour
{
    [Header("General")]
    [SerializeField]
    [Range(0.01f, 0.2f)]
    float dotsSoundInterval = 0.1f;
    [HideInInspector]
    public float lineMaxLenght;
    float lastDotPercentageSound;
    float currentDotPercentage;
    Vector3 lineEnd;

    [HideInInspector]
    public GameObject mouseProjection;
    SpriteRenderer[] spriteRenderers;
    List<MapElement> elementsInSight;

    [HideInInspector]
    public bool inATyphoon;
    [HideInInspector]
    public bool onAnIsland;
    [HideInInspector]
    public Island currentIsland;

    [HideInInspector]
    public bool safeZone;

    [Header("References")]
    [SerializeField]
    NavigationManager navigationManager;
    [SerializeField]
    LineRenderer line;
    [SerializeField]
    GameObject endOfLine;
    [SerializeField]
    LineRenderer trail;
    [SerializeField]
    Transform trailPosFolder;

    int trailPosCount;
    Vector3 currentTrailPos;

    void Start()
    {
        line.SetPosition(0, new Vector3(0, 0, -0.1f));
        line.enabled = false;
        endOfLine.SetActive(false);
        elementsInSight = new List<MapElement>();
        inATyphoon = false;
        onAnIsland = false;
        safeZone = false;

        trailPosCount = 1;
        trail.positionCount = trailPosCount;

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void AddTrailPos()
    {
        GameObject newTrailPos = new GameObject("TrailPosition");
        newTrailPos.transform.parent = trailPosFolder;
        newTrailPos.transform.position = transform.position;
        trailPosCount++;
    }

    public void StartTargeting()
    {
        line.enabled = true;
        endOfLine.SetActive(true);
        AkSoundEngine.PostEvent("Play_Sailing", gameObject);
        currentDotPercentage = 0;
        PlayDotsSound();
    }

    public void StopTargeting()
    {
        line.enabled = false;
        endOfLine.SetActive(false);
    }

    void Update()
    {
        // Update enavigation line
        if (line.enabled)
        {
            lineEnd = transform.InverseTransformPoint(navigationManager.lastValidCursorPos);
            lineEnd.z = -0.1f;
            line.SetPosition(1, lineEnd);
            endOfLine.transform.localPosition = lineEnd;
            currentDotPercentage = (navigationManager.lastValidCursorPos - transform.position).sqrMagnitude / lineMaxLenght;
            if (Mathf.Abs(lastDotPercentageSound - currentDotPercentage) >= dotsSoundInterval)
                PlayDotsSound();
        }

        // Update trail positions
        trail.positionCount = trailPosCount;
        for (int i = 0; i < trailPosCount - 1; i ++)
            trail.SetPosition(i, trailPosFolder.GetChild(i).transform.position);
        trail.SetPosition(trailPosCount - 1, transform.position);
    }

    void PlayDotsSound()
    {
        AkSoundEngine.SetRTPCValue("Navigation_Distance", currentDotPercentage);
        AkSoundEngine.PostEvent("Play_Dots", gameObject);
        lastDotPercentageSound = currentDotPercentage;
    }

    public void ElementInSight(MapElement element, bool needTelescopeRefresh = false)
    {
        if (!elementsInSight.Contains(element))
            elementsInSight.Add(element);

        if (needTelescopeRefresh)
            EventManager.Instance.Raise(new BoatAsksTelescopeRefreshEvent() { element = element });

        if (element.magnetism)
            EventManager.Instance.Raise(new BoatInMapElementEvent() { exit = false, elementZone = element.GetComponentInChildren<ElementViewZone>() });  
    }

    public void ElementNoMoreInSight(MapElement element)
    {
        if (elementsInSight.Contains(element))
            elementsInSight.Remove(element);

        if (element.magnetism)
            EventManager.Instance.Raise(new BoatInMapElementEvent() { exit = true, elementZone = element.GetComponentInChildren<ElementViewZone>() });
    }

    public List<MapElement> GetElementsInSight()
    {
        return elementsInSight;
    }

    public void SetImageAlpha(bool dark)
    {
        float colorChange = dark ? -0.4f : 0.4f;
        Color color;

        foreach (SpriteRenderer sprite in spriteRenderers)
        {
            color = sprite.color;
            color.r += colorChange; 
            color.g += colorChange; 
            color.b += colorChange;
            sprite.color = color;
        }
    }
}
