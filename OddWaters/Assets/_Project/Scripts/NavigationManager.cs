using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Aura2API;

public enum ENavigationResult
{
    SEA,
    ISLAND,
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
    Boat boatScript;
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
    bool hasPlayedArrivalTransition;
    GameObject lastValidPosition;
    int lastValidZone;

    [SerializeField]
    Telescope telescope;
    
    bool onIsland = false;
    int targetZone;

    Vector3 boatColliderLeft;
    Vector3 boatColliderRight;
    bool goingIntoTyphoon = false;

    [HideInInspector]
    public Vector3 obstaclePos;

    void Start()
    {
        boatRenderer = boat.GetComponent<SpriteRenderer>();
        boatScript = boat.GetComponent<Boat>();
        navigating = false;
        hasPlayedArrivalTransition = false;

        lastValidPosition = new GameObject("BoatGhost");
        lastValidPosition.transform.parent = map.gameObject.transform;
        lastValidPosition.transform.position = boat.transform.position;

        CapsuleCollider capsuleCollider = boat.transform.GetComponentInChildren<BoatWorldCollider>().GetComponent<CapsuleCollider>();
        Vector3 extent = boat.transform.right * capsuleCollider.radius;
        boatColliderLeft = boat.transform.position - extent;
        boatColliderRight = boat.transform.position + extent;

        telescope.RefreshElements(boat.transform.up, boat.transform.position, boat.transform.right, map.GetCurrentPanorama());
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
            journeyTarget.y = boat.transform.position.y;
            Vector3 journey = journeyTarget - boat.transform.position;

            // End of journey
            if (journey.sqrMagnitude <= 0.0005f)
            {
                navigating = false;

                if (!hasPlayedArrivalTransition)
                    PlayEndAnimation();

                // If correct position
                if (!boatScript.inATyphoon)
                {
                    // Save position
                    lastValidPosition.transform.position = boat.transform.position;
                    lastValidZone = map.currentZone;

                    // Berth on island if needed
                    if (boatScript.onAnIsland && boatScript.currentIsland.islandNumber != screenManager.currentIslandNumber)
                    {
                        BerthOnIsland();
                    }
                    else
                    {
                        //EndJourneyAtSea
                        screenManager.EndNavigationAtSea();
                        EventManager.Instance.Raise(new BlockInputEvent() { block = false });
                    }
                        
                }
            }
            // Still journeying
            else
            {
                float distCovered = (Time.time - journeyBeginTime) * boatSpeed;
                float fracJourney = distCovered / journeyLength;
                boat.transform.position = Vector3.Lerp(boat.transform.position, journeyTarget, fracJourney);

                // Near the end of journey
                if (journey.sqrMagnitude <= 0.1f && !goingIntoTyphoon && !hasPlayedArrivalTransition)
                    PlayEndAnimation();
            }
        }
    }

    public void NavigateToPosition(Vector3 targetPos, int zoneNumber)
    {
        Vector3 journey = targetPos - boat.transform.position;
        if (journey.sqrMagnitude >= minDistance * minDistance)
            LaunchNavigation(targetPos, zoneNumber);
    }

    void OnBoatInTyphoonEvent(BoatInTyphoonEvent e)
    {
        Debug.Log("Boat in typhoon!");
        StartCoroutine(WaitBeforeGoingToInitialPos());
    }

    IEnumerator WaitBeforeGoingToInitialPos()
    {
        yield return new WaitForSeconds(0.5f);
        LaunchNavigation(lastValidPosition.transform.position, lastValidZone, true);
    }

    void LaunchNavigation(Vector3 target, int newZoneNumber, bool fromTyphoon = false)
    {
        EventManager.Instance.Raise(new BlockInputEvent() { block = true });

        // Initialize navigation values
        navigating = true;
        boatRenderer.sprite = boatSprites[0];
        boat.transform.GetChild(0).gameObject.SetActive(true);
        lightScript.rotateDegreesPerSecond.value.y = sunMove;
        target.y = boat.transform.position.y;
        journeyLength = (target - boat.transform.position).sqrMagnitude;
        journeyTarget = target;
        journeyBeginTime = Time.time;
        hasPlayedArrivalTransition = false;

        // Update map zone
        if (newZoneNumber != map.currentZone)
            map.ChangeZone(newZoneNumber);

        if (!fromTyphoon)
        {
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

            // Dezoom
            telescope.ResetZoom();

            if (onIsland)
            {
                onIsland = false;
                screenManager.LeaveIsland();
            } 
            else
                screenManager.BeginNavigation();

            // Check if typhoon on journey
            bool typhoonOnRight = false;
            Vector3 raycastDir = journeyTarget - boatColliderLeft;
            float raycastLenght = raycastDir.magnitude;
            bool typhoonOnLeft = Physics.RaycastAll(boatColliderLeft, raycastDir, raycastLenght).Any(hit => hit.collider.CompareTag("Typhoon"));
            if (!typhoonOnLeft)
            {
                raycastDir = journeyTarget - boatColliderRight;
                typhoonOnRight = Physics.RaycastAll(boatColliderRight, raycastDir, raycastLenght).Any(hit => hit.collider.CompareTag("Typhoon"));
            }
            goingIntoTyphoon = (typhoonOnLeft || typhoonOnRight);
            Debug.Log("Going into a typhoon? " + goingIntoTyphoon);
        }
    }

    void PlayEndAnimation()
    {
        hasPlayedArrivalTransition = true;

        AkSoundEngine.PostEvent("Play_Arrival", gameObject);
        lightScript.rotateDegreesPerSecond.value.y = 0;
        
        telescope.RefreshElements(boat.transform.up, journeyTarget, boat.transform.right, map.GetCurrentPanorama());
    }

    void BerthOnIsland()
    {
        onIsland = true;
        boatRenderer.sprite = boatSprites[1];

        // Reset rotations
        boat.transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, 0);
        boat.transform.GetChild(0).gameObject.SetActive(false);
        boat.transform.localRotation = Quaternion.Euler(90, 0, 0);

        StartCoroutine(screenManager.Berth(boatScript.currentIsland));
    }

    public void UpdateNavigation(Vector3 targetPos)
    {
        ENavigationResult result = GetNavigationResult(targetPos);
        switch (result)
        {
            case ENavigationResult.SEA:
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

        targetPos.y += boat.transform.localPosition.y;
        Vector3 journey = targetPos - boat.transform.position;
        float distance = journey.magnitude;

        Vector3 rayOrigin = targetPos;
        rayOrigin.y += 1;
        RaycastHit[] hitsAtTarget = Physics.RaycastAll(rayOrigin, new Vector3(0, -1, 0), 5);

        bool endOfMap = !hitsAtTarget.Any(hit => hit.collider.GetComponent<MapZone>() && hit.collider.GetComponent<MapZone>().visible);
        RaycastHit[] hitsOnJourney = Physics.RaycastAll(targetPos, -journey, distance);
        RaycastHit mapEnd = hitsOnJourney.FirstOrDefault(hit => hit.collider.GetComponent<MapZone>() && hit.collider.GetComponent<MapZone>().zoneNumber == map.currentZone);
        hitsOnJourney = Physics.RaycastAll(boat.transform.position, journey, distance);

        if (distance <= maxDistance)
        {
            // Visible island at target position (ok)
            if (hitsAtTarget.Any(hit => hit.collider.GetComponent<Island>() && hit.collider.GetComponent<Island>().visible))
                return ENavigationResult.ISLAND;

            RaycastHit obstacle = hitsOnJourney.FirstOrDefault(hit => (hit.collider.GetComponent<Island>() && hit.collider.GetComponent<Island>().visible && hit.collider.GetComponent<Island>().islandNumber != screenManager.currentIslandNumber) || (hit.collider.GetComponent<MapZone>() && !hit.collider.GetComponent<MapZone>().visible));

            // Visible island or invisible map zone on trajectory (ko)
            if (obstacle.collider)
            {
                obstaclePos = obstacle.point;
                return ENavigationResult.KO;
            }

            // No map (ko)
            if (endOfMap && mapEnd.collider)
            {
                obstaclePos = mapEnd.point;
                return ENavigationResult.KO;
            }

            // Visible map zone (ok)
            RaycastHit mapZone = hitsAtTarget.FirstOrDefault(hit => hit.collider.GetComponent<MapZone>() && hit.collider.GetComponent<MapZone>().visible);
            if (mapZone.collider)
            {
                targetZone = mapZone.collider.GetComponent<MapZone>().zoneNumber;
                return ENavigationResult.SEA;
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

            // No map (ko)
            if (endOfMap && mapEnd.collider && (mapEnd.point - boat.transform.position).sqrMagnitude < (obstaclePos - boat.transform.position).sqrMagnitude)
            {
                obstaclePos = mapEnd.point;
                return ENavigationResult.KO;
            }

            return ENavigationResult.KO;
        }

        Debug.Log("???");
        return ENavigationResult.KO;
    }
}
