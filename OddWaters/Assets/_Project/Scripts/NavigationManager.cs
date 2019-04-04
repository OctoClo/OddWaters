using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Aura2API;

public enum ENavigationResult { SEA, ISLAND, KO };

public class NavigationManager : MonoBehaviour
{
    [SerializeField]
    ScreenManager screenManager;

    [SerializeField]
    AutoMoveAndRotate lightScript;
    [SerializeField]
    int sunMove = -3;

    [SerializeField]
    Map map;

    [SerializeField]
    GameObject boat;
    float boatPosY;
    SpriteRenderer boatRenderer;
    [SerializeField]
    Sprite[] boatSprites;
    [SerializeField]
    float boatSpeed;

    bool navigating;
    Vector3 journeyTarget;
    float journeyBeginTime;
    float journeyLength;
    bool hasPlayedAnim;
    Island islandTarget;

    [SerializeField]
    Telescope telescope;

    int currentZone = -1;

    void Start()
    {
        boatRenderer = boat.GetComponent<SpriteRenderer>();
        boatPosY = boat.transform.position.y;
        navigating = false;
        hasPlayedAnim = false;
        islandTarget = null;
    }

    void Update()
    {
        if (navigating)
        {
            if (Vector3.Distance(boat.transform.position, journeyTarget) <= 0.1f)
            {
                navigating = false;
                lightScript.rotateDegreesPerSecond.value.y = 0;
                if (islandTarget)
                {
                    boatRenderer.sprite = boatSprites[1];
                    screenManager.Berth(islandTarget);
                    islandTarget.Berth();
                    islandTarget = null;
                }
                else
                    EventManager.Instance.Raise(new BlockInputEvent() { block = false });
            }
            else
            {
                float distCovered = (Time.time - journeyBeginTime) * boatSpeed;
                float fracJourney = distCovered / journeyLength;
                boat.transform.position = Vector3.Lerp(boat.transform.position, journeyTarget, fracJourney);
                if (Vector3.Distance(boat.transform.position, journeyTarget) <= 1f && !hasPlayedAnim && !islandTarget)
                {
                    hasPlayedAnim = true;
                    telescope.ResetZoom();
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

    void LaunchNavigation(Vector3 target)
    {
        navigating = true;
        EventManager.Instance.Raise(new BlockInputEvent() { block = true });
        boatRenderer.sprite = boatSprites[0];
        lightScript.rotateDegreesPerSecond.value.y = sunMove;
        journeyLength = Vector3.Distance(target, boat.transform.position);
        journeyTarget = target;
        journeyTarget.y = boatPosY;
        journeyBeginTime = Time.time;
    }

    public void NavigateToZone(Vector3 targetPos, int zoneNumber)
    {
        if (Vector3.Distance(targetPos, boat.transform.position) >= 1f)
        {
            LaunchNavigation(targetPos);
            telescope.PlayAnimation(true, false);
            hasPlayedAnim = false;
            
            if (zoneNumber != map.currentZone)
                map.currentZone = zoneNumber;
        }
    }

    public void NavigateToIsland(Island island)
    {
        LaunchNavigation(island.transform.position);
        islandTarget = island;

        if (island.islandNumber != map.currentZone)
            map.currentZone = island.islandNumber;
    }
}
