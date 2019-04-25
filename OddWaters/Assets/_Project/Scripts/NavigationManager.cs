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
    [SerializeField]
    float maxDistance = 3f;

    bool navigating;
    Vector3 journeyTarget;
    float journeyBeginTime;
    float journeyLength;
    bool hasPlayedAnim;
    Island islandTarget;
    bool navigatingToTyphoon;
    Vector3 initialPos;

    [SerializeField]
    Telescope telescope;
    
    bool onIsland = false;
    int currentZone = -1;

    [HideInInspector]
    public Vector3 obstaclePos;

    void Start()
    {
        boatRenderer = boat.GetComponent<SpriteRenderer>();
        initialPos = boat.transform.position;
        boatPosY = initialPos.y;
        navigating = false;
        hasPlayedAnim = false;
        islandTarget = null;
    }

    void OnEnable()
    {
        EventManager.Instance.AddListener<BoatInTyphoonEvent>(OnBoatInTyphoonEvent);    
    }

    void OnDisable()
    {
        EventManager.Instance.RemoveListener<BoatInTyphoonEvent>(OnBoatInTyphoonEvent);
    }

    void Update()
    {
        if (navigating)
        {
            Vector3 journey = journeyTarget - boat.transform.position;

            // End of journey
            if (journey.sqrMagnitude <= 0.001f)
            {
                navigating = false;
                lightScript.rotateDegreesPerSecond.value.y = 0;
                if (islandTarget)
                {
                    onIsland = true;
                    boatRenderer.sprite = boatSprites[1];

                    // Reset rotations
                    boat.transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, 0);
                    boat.transform.GetChild(0).gameObject.SetActive(false);
                    boat.transform.localRotation = Quaternion.Euler(90, 0, 0);

                    if (islandTarget.firstTimeVisiting)
                    {
                        telescope.ResetAnimation();
                        screenManager.Berth(islandTarget);
                    }

                    islandTarget.Berth();
                    islandTarget = null;
                }
                else
                    EventManager.Instance.Raise(new BlockInputEvent() { block = false });
            }
            // Still journeying
            else
            {
                float distCovered = (Time.time - journeyBeginTime) * boatSpeed;
                float fracJourney = distCovered / journeyLength;
                boat.transform.position = Vector3.Lerp(boat.transform.position, journeyTarget, fracJourney);
                // Near the end of journey
                if (journey.sqrMagnitude <= 0.1f && !hasPlayedAnim && (!islandTarget || islandTarget && !islandTarget.firstTimeVisiting) && !navigatingToTyphoon)
                {
                    hasPlayedAnim = true;
                    telescope.PlayAnimation(false, true);
                    telescope.RefreshElements(boat.transform.up, journeyTarget, boat.transform.right, map.GetCurrentPanorama());
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

    public void UpdateNavigation(Vector3 targetPos)
    {
        ENavigationResult result = GetNavigationResult(targetPos);
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
        obstaclePos = Vector3.zero;

        Vector3 journey = targetPos - boat.transform.position;
        float distance = journey.magnitude;

        Vector3 rayOrigin = targetPos;
        rayOrigin.y += 1;

        RaycastHit[] hitsAtTarget = Physics.RaycastAll(rayOrigin, new Vector3(0, -1, 0), 5);
        // Visible island at target position (ok)
        if (hitsAtTarget.Any(hit => hit.collider.GetComponent<Island>() && hit.collider.GetComponent<Island>().visible) && distance <= maxDistance)
            return ENavigationResult.ISLAND;
        
        RaycastHit[] hitsOnJourney = Physics.RaycastAll(boat.transform.position, journey, distance);
        RaycastHit obstacle = hitsOnJourney.FirstOrDefault(hit => (hit.collider.GetComponent<Island>() && hit.collider.GetComponent<Island>().visible && hit.collider.GetComponent<Island>().islandNumber != screenManager.currentIslandNumber) || (hit.collider.GetComponent<MapZone>() && !hit.collider.GetComponent<MapZone>().visible));
        // Visible island or invisible map zone on trajectory (ko)
        if (obstacle.collider)
        {
            obstaclePos = obstacle.point;
            return ENavigationResult.KO;
        }
        else if (distance <= maxDistance)
        {
            // Typhoon (ok)
            if (hitsOnJourney.Any(hit => hit.collider.CompareTag("Typhoon")))
                return ENavigationResult.TYPHOON;
            // Visible map zone (ok)
            else if (hitsAtTarget.Any(hit => hit.collider.GetComponent<MapZone>() && hit.collider.GetComponent<MapZone>().visible))
                return ENavigationResult.SEA;
            // No map (ko)
            else
            {
                hitsOnJourney = Physics.RaycastAll(targetPos, -journey, distance);
                RaycastHit endOfMap = hitsOnJourney.FirstOrDefault(hit => hit.collider.GetComponent<MapZone>() && hit.collider.GetComponent<MapZone>().zoneNumber == map.currentZone);
                obstaclePos = endOfMap.point;
                return ENavigationResult.KO;
            }
        }
        else
        {
            // Too far (ko)
            float factor = maxDistance / journey.magnitude;
            float x = (targetPos.x - boat.transform.position.x) * factor + boat.transform.position.x;
            float y = (targetPos.y - boat.transform.position.y) * factor + boat.transform.position.y;
            float z = (targetPos.z - boat.transform.position.z) * factor + boat.transform.position.z;
            obstaclePos = new Vector3(x, y, z);
            return ENavigationResult.KO;
        }
    }

    void LaunchNavigation(Vector3 target)
    {
        EventManager.Instance.Raise(new BlockInputEvent() { block = true });
        AkSoundEngine.PostEvent("Play_Departure", gameObject);
        
        // Rotate boat
        boat.transform.LookAt(target);
        Vector3 rotation = boat.transform.eulerAngles;
        rotation.z = -rotation.y;
        rotation.x = 90;
        rotation.y = 0;
        boat.transform.eulerAngles = rotation;

        // Reset field of view rotation
        boat.transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, 0);

        // Initialize navigation values
        navigating = true;
        boatRenderer.sprite = boatSprites[0];
        boat.transform.GetChild(0).gameObject.SetActive(true);
        lightScript.rotateDegreesPerSecond.value.y = sunMove;
        journeyLength = (target - boat.transform.position).sqrMagnitude;
        journeyTarget = target;
        journeyTarget.y = boatPosY;
        journeyBeginTime = Time.time;

        // Dezoom and fade out
        telescope.PlayAnimation(true, false);
        telescope.ResetZoom();
        hasPlayedAnim = false;
    }

    public void NavigateToPosition(Vector3 targetPos, int zoneNumber)
    {
        Vector3 journey = targetPos - boat.transform.position;
        if (journey.sqrMagnitude >= minDistance * minDistance)
        {
            LaunchNavigation(targetPos);

            if (zoneNumber != map.currentZone)
                map.ChangeZone(zoneNumber);
        }
    }

    public void NavigateToIsland(Island island)
    {
        LaunchNavigation(island.transform.position);
        islandTarget = island;

        if (island.islandNumber != map.currentZone)
            map.ChangeZone(island.islandNumber);
    }

    public void NavigateToTyphoon(Vector3 targetPos)
    {
        Vector3 journey = targetPos - boat.transform.position;
        if (journey.sqrMagnitude >= minDistance * minDistance)
        {
            LaunchNavigation(targetPos);
            navigatingToTyphoon = true;
            initialPos = boat.transform.position;
        }
    }

    void OnBoatInTyphoonEvent(BoatInTyphoonEvent e)
    {
        journeyLength = (boat.transform.position - initialPos).sqrMagnitude;
        journeyTarget = initialPos;
        journeyBeginTime = Time.time;
    }
}
