using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ENavigationResult { SEA, ISLAND, KO };

public class NavigationManager : MonoBehaviour
{
    [SerializeField]
    Map map;

    [SerializeField]
    GameObject boat;
    float boatPosY;

    [SerializeField]
    Telescope telescope;

    int currentZone = -1;

    void Start()
    {
        boatPosY = boat.transform.position.y;
    }

    void Update()
    {
        if (map.currentZone != currentZone)
        {
            currentZone = map.currentZone;
            Sprite newSprite = map.GetCurrentZoneSprite();
            StartCoroutine(telescope.ChangeSprite(newSprite));
        }
    }

    public ENavigationResult GetNavigationResult(Vector3 targetPos)
    {
        Vector3 rayOrigin = targetPos;
        rayOrigin.y += 1;
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, new Vector3(0, -1, 0), 5);

        if (hits.Any(hit => hit.collider.GetComponent<Island>()))
        {
            return ENavigationResult.ISLAND;
        }
        else
        {
            Vector3 direction = (targetPos - boat.transform.position);
            float distance = direction.magnitude;
            direction.Normalize();
            hits = Physics.RaycastAll(boat.transform.position, direction, distance);
            if (hits.Any(hit => hit.collider.GetComponent<Island>() || (hit.collider.GetComponent<MapZone>() && !hit.collider.GetComponent<MapZone>().visible)))
            {
                return ENavigationResult.KO;
            }
            else
            {
                return ENavigationResult.SEA;
            }
        }
    }

    public void NavigateTo(Vector3 targetPos)
    {
        if ((targetPos - boat.transform.position).magnitude >= 0.5f)
        {
            targetPos.y = boatPosY;
            boat.transform.position = targetPos;
        }
    }
}
