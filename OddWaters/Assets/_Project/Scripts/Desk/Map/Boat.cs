using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour
{
    [SerializeField]
    NavigationManager navigationManager;

    [SerializeField]
    LineRenderer line;
    [HideInInspector]
    public GameObject mouseProjection;
    SpriteRenderer[] spriteRenderers;

    List<Island> islandsInSight;

    [HideInInspector]
    public bool inATyphoon;

    [HideInInspector]
    public bool onAnIsland;
    [HideInInspector]
    public Island currentIsland;

    void Start()
    {
        line.enabled = false;
        islandsInSight = new List<Island>();
        inATyphoon = false;
        onAnIsland = false;

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void StartTargeting()
    {
        line.enabled = true;
    }

    public void StopTargeting()
    {
        line.enabled = false;
    }

    void Update()
    {
        if (line.enabled)
        {
            line.SetPosition(0, transform.position);
            line.SetPosition(1, navigationManager.lastValidTarget);
        }
    }

    public void IslandInSight(Island island)
    {
        if (!islandsInSight.Contains(island))
            islandsInSight.Add(island);
    }

    public void IslandNoMoreInSight(Island island)
    {
        if (islandsInSight.Contains(island))
            islandsInSight.Remove(island);
    }

    public List<Island> GetIslandsInSight()
    {
        return islandsInSight;
    }

    public void SetImageAlpha(bool dark)
    {
        float alpha = dark ? 0.3f : 1;
        Color color = new Color(alpha, alpha, alpha, 1);

        foreach (SpriteRenderer sprite in spriteRenderers)
            sprite.color = color;
    }
}
