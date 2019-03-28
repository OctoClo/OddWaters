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
    float boatSpeed;
    bool navigating;
    Vector3 journeyTarget;
    float journeyBeginTime;
    float journeyLength;
    bool hasPlayedAnim;

    [SerializeField]
    Telescope telescope;

    int currentZone = -1;

    void Start()
    {
        boatPosY = boat.transform.position.y;
        navigating = false;
        hasPlayedAnim = false;
    }

    void Update()
    {
        if (navigating)
        {
            if (Vector3.Distance(boat.transform.position, journeyTarget) <= 0.1f)
            {
                navigating = false;
                EventManager.Instance.Raise(new BlockInputEvent() { block = false });
            }
            else
            {
                float distCovered = (Time.time - journeyBeginTime) * boatSpeed;
                float fracJourney = distCovered / journeyLength;
                boat.transform.position = Vector3.Lerp(boat.transform.position, journeyTarget, fracJourney);
                if (Vector3.Distance(boat.transform.position, journeyTarget) <= 1f && !hasPlayedAnim)
                {
                    hasPlayedAnim = true;
                    telescope.PlayAnimation(false, true, map.GetCurrentZoneSprite());
                }
            }
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

    public void NavigateTo(Vector3 targetPos, int zoneNumber)
    {
        journeyLength = Vector3.Distance(targetPos, boat.transform.position);
        if (journeyLength >= 0.5f)
        {
            EventManager.Instance.Raise(new BlockInputEvent() { block = true });
            navigating = true;
            journeyTarget = targetPos;
            journeyTarget.y = boatPosY;
            journeyBeginTime = Time.time;
            telescope.PlayAnimation(true, false);
            hasPlayedAnim = false;

            if (zoneNumber != map.currentZone)
                map.currentZone = zoneNumber;
        }
    }
}
