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
    Gradient[] colorGradients;
    [HideInInspector]
    public GameObject mouseProjection;

    List<Island> islandsInSight;

    [HideInInspector]
    public bool inATyphoon;

    void Start()
    {
        line.enabled = false;
        islandsInSight = new List<Island>();
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
            Vector3 obstacle = navigationManager.obstaclePos;
            if (obstacle != Vector3.zero)
            {
                line.colorGradient = colorGradients[1];
                line.SetPosition(1, obstacle);
            }
            else
            {
                line.colorGradient = colorGradients[0];
                Vector3 endPos = mouseProjection.transform.position;
                endPos.y += transform.localPosition.y;
                line.SetPosition(1, endPos);
            }
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
}
