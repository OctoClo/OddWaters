using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour
{
    [SerializeField]
    NavigationManager navigationManager;

    [SerializeField]
    LineRenderer line;
    [SerializeField]
    GameObject endOfLine;
    [HideInInspector]
    public float lineMaxLenght;
    float lastDotPercentageSound;
    float currentDotPercentage;
    [SerializeField]
    [Range(0.01f, 0.2f)]
    float dotsSoundInterval = 0.1f;

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

    void Start()
    {
        line.enabled = false;
        endOfLine.SetActive(false);
        elementsInSight = new List<MapElement>();
        inATyphoon = false;
        onAnIsland = false;

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
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
        if (line.enabled)
        {
            line.SetPosition(0, transform.position);
            line.SetPosition(1, navigationManager.lastValidCursorPos);
            endOfLine.transform.position = navigationManager.lastValidCursorPos;
            currentDotPercentage = (navigationManager.lastValidCursorPos - transform.position).sqrMagnitude / lineMaxLenght;
            if (Mathf.Abs(lastDotPercentageSound - currentDotPercentage) >= dotsSoundInterval)
                PlayDotsSound();
        }
    }

    void PlayDotsSound()
    {
        AkSoundEngine.SetRTPCValue("Navigation_Distance", currentDotPercentage);
        AkSoundEngine.PostEvent("Play_Dots", gameObject);
        lastDotPercentageSound = currentDotPercentage;
    }

    public void ElementInSight(MapElement element)
    {
        if (!elementsInSight.Contains(element))
            elementsInSight.Add(element);
    }

    public void ElementNoMoreInSight(MapElement element)
    {
        if (elementsInSight.Contains(element))
            elementsInSight.Remove(element);
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
