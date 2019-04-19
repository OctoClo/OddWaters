using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatInTyphoonEvent : GameEvent { };

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

    void Start()
    {
        line.enabled = false;
        islandsInSight = new List<Island>();
    }

    public void StartTargeting()
    {
        line.SetPosition(0, transform.position);
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
            Vector3 obstacle = navigationManager.obstaclePos;
            if (obstacle != Vector3.zero)
            {
                obstacle.y = 0.3f;
                line.colorGradient = colorGradients[1];
                line.SetPosition(1, obstacle);
            }
            else
            {
                line.colorGradient = colorGradients[0];
                line.SetPosition(1, mouseProjection.transform.position);
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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Typhoon"))
            EventManager.Instance.Raise(new BoatInTyphoonEvent());
    }
}
