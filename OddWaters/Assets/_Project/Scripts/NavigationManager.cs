using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Aura2API;

public enum ENavigationResult
{
    SEA,
    ISLAND,
    TYPHOON,
    KO
}

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
    [SerializeField]
    float minDistance = 1f;

    bool navigating;
    Vector3 journeyTarget;
    float journeyBeginTime;
    float journeyLength;
    bool hasPlayedAnim;
    Island islandTarget;

    [SerializeField]
    Telescope telescope;

    bool onIsland = false;
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
            if (Vector3.Distance(boat.transform.position, journeyTarget) <= 0.1f) // End of journey
            {
                navigating = false;
                lightScript.rotateDegreesPerSecond.value.y = 0;
                if (islandTarget)
                {
                    boatRenderer.sprite = boatSprites[1];
                    onIsland = true;

                    if (islandTarget.firstTimeVisiting)
                    {
                        ResetTelescopeAnimation();
                        screenManager.Berth(islandTarget);
                    }

                    islandTarget.Berth();
                    islandTarget = null;
                }
                else
                    EventManager.Instance.Raise(new BlockInputEvent() { block = false });
            }
            else // Still journeying
            {
                float distCovered = (Time.time - journeyBeginTime) * boatSpeed;
                float fracJourney = distCovered / journeyLength;
                boat.transform.position = Vector3.Lerp(boat.transform.position, journeyTarget, fracJourney);
                if (Vector3.Distance(boat.transform.position, journeyTarget) <= 1f && !hasPlayedAnim && (!islandTarget || islandTarget && !islandTarget.firstTimeVisiting)) // Near the end of journey
                {
                    hasPlayedAnim = true;
                    telescope.ResetZoom();
                    telescope.PlayAnimation(false, true, map.GetCurrentZoneSprite());
                    if (islandTarget && !islandTarget.firstTimeVisiting)
                        screenManager.Berth(islandTarget);
                    if (onIsland)
                    {
                        screenManager.LeaveIsland();
                        onIsland = false;
                    }
                }
            }
        }
    }

    public void SetCursorNavigation(Vector3 currentPos)
    {
        ENavigationResult result = GetNavigationResult(currentPos);
        switch (result)
        {
            case ENavigationResult.SEA:
            case ENavigationResult.TYPHOON:
                CursorManager.Instance.SetCursor(ECursor.NAVIGATION_OK);
                break;

            case ENavigationResult.ISLAND:
                CursorManager.Instance.SetCursor(ECursor.NAVIGATION_ISLAND);
                break;

            case ENavigationResult.KO:
                CursorManager.Instance.SetCursor(ECursor.NAVIGATION_KO);
                break;
        }
    }

    public ENavigationResult GetNavigationResult(Vector3 targetPos)
    {
        Vector3 rayOrigin = targetPos;
        rayOrigin.y += 1;
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, new Vector3(0, -1, 0), 5);

        if (hits.Any(hit => hit.collider.GetComponent<Island>()))
            return ENavigationResult.ISLAND;
        else
        {
            Vector3 direction = (targetPos - boat.transform.position);
            float distance = direction.magnitude;
            direction.Normalize();
            hits = Physics.RaycastAll(boat.transform.position, direction, distance);
            if (hits.Any(hit => hit.collider.GetComponent<Island>() || (hit.collider.GetComponent<MapZone>() && !hit.collider.GetComponent<MapZone>().visible)))
                return ENavigationResult.KO;
            else if (hits.Any(hit => hit.collider.CompareTag("Typhoon")))
                return ENavigationResult.TYPHOON;
            else
                return ENavigationResult.SEA;
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
        telescope.PlayAnimation(true, false);
        hasPlayedAnim = false;
    }

    public void NavigateToPosition(Vector3 targetPos, int zoneNumber)
    {
        if (Vector3.Distance(targetPos, boat.transform.position) >= minDistance)
        {
            LaunchNavigation(targetPos);
            
            if (zoneNumber != map.currentZone)
                map.currentZone = zoneNumber;
        }
    }

    public void NavigateToIsland(Island island)
    {
        if (Vector3.Distance(island.transform.position, boat.transform.position) >= minDistance)
        {
            LaunchNavigation(island.transform.position);
            islandTarget = island;

            if (island.islandNumber != map.currentZone)
                map.currentZone = island.islandNumber;
        }
    }

    void ResetTelescopeAnimation()
    {
        telescope.ResetAnimation();
    }
}
