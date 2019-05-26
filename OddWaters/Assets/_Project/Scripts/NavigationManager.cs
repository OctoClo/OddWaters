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
    float boatSpeedFromTyhpoon;
    float currentSpeed;
    [SerializeField]
    float maxDistance = 3f;
    float maxDistanceSqr;
    [SerializeField]
    LineRenderer boatTrail;
    int linePoints = 0;

    bool navigating;
    Vector3 journeyTarget;
    float journeyBeginTime;
    float journeyLength;
    GameObject lastValidPosition;
    int lastValidPositionZone;

    [SerializeField]
    Telescope telescope;
    
    bool onIsland = false;

    Vector3 boatColliderLeft;
    Vector3 boatColliderRight;
    bool goingIntoTyphoon = false;
    bool fromTyphoon = false;

    [HideInInspector]
    public Vector3 lastValidCursorPos;
    Vector3 lastValidTarget;
    int lastValidTargetZone;

    [SerializeField]
    TutorialManager tutorialManager;
    [HideInInspector]
    public Collider goalCollider;
    bool insideGoal;

    void Start()
    {
        boatRenderer = boat.GetComponent<SpriteRenderer>();
        boatScript = boat.GetComponent<Boat>();
        navigating = false;
        insideGoal = false;

        lastValidPosition = new GameObject("BoatGhost");
        lastValidPosition.transform.parent = map.gameObject.transform;
        lastValidPosition.transform.position = boat.transform.position;

        CapsuleCollider capsuleCollider = boat.transform.GetComponentInChildren<BoatWorldCollider>().GetComponent<CapsuleCollider>();
        Vector3 extent = boat.transform.right * capsuleCollider.radius;
        boatColliderLeft = boat.transform.position - extent;
        boatColliderRight = boat.transform.position + extent;
        boatTrail.SetPosition(0, boat.transform.position);

        maxDistanceSqr = maxDistance * maxDistance;
        boatScript.lineMaxLenght = maxDistanceSqr;
    }

    void OnEnable()
    {
        EventManager.Instance.AddListener<BoatInTyphoonEvent>(OnBoatInTyphoonEvent);    
        EventManager.Instance.AddListener<BoatInMapElement>(OnBoatInMapElement);    
    }

    void OnDisable()
    {
        EventManager.Instance.RemoveListener<BoatInTyphoonEvent>(OnBoatInTyphoonEvent);
        EventManager.Instance.RemoveListener<BoatInMapElement>(OnBoatInMapElement);
    }

    public IEnumerator InitializeTelescopeElements()
    {
        yield return new WaitForSeconds(0.1f);
        telescope.RefreshElements(boat.transform.up, boat.transform.position, boat.transform.right, map.GetCurrentPanorama());
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
                
                // If correct position
                if (!boatScript.inATyphoon)
                {
                    AkSoundEngine.PostEvent("Play_Arrival", gameObject);
                    AkSoundEngine.PostEvent("Stop_Typhon", gameObject);
                    lightScript.rotateDegreesPerSecond.value.y = 0;
                    telescope.RefreshElements(boat.transform.up, journeyTarget, boat.transform.right, map.GetCurrentPanorama());

                    // Save position
                    lastValidPosition.transform.position = boat.transform.position;
                    lastValidPositionZone = map.currentZone;

                    // Berth on island if needed
                    if (boatScript.onAnIsland && boatScript.currentIsland.islandNumber != screenManager.currentIslandNumber && !fromTyphoon &&
                        (tutorialManager.step == ETutorialStep.NO_TUTORIAL || tutorialManager.step == ETutorialStep.GO_TO_ISLAND))
                        BerthOnIsland((goalCollider != null));
                    else
                    {
                        //EndJourneyAtSea
                        screenManager.EndNavigationAtSea();
                        EventManager.Instance.Raise(new BlockInputEvent() { block = false, navigation = true });

                        if (goalCollider && insideGoal)
                            tutorialManager.NextStep();
                    }

                    fromTyphoon = false;
                    if (goalCollider && insideGoal)
                    {
                        goalCollider = null;
                        insideGoal = false;
                    }
                }
            }
            // Still journeying
            else
            {
                float distCovered = (Time.time - journeyBeginTime) * currentSpeed;
                float fracJourney = distCovered / journeyLength;
                boat.transform.position = Vector3.Lerp(boat.transform.position, journeyTarget, fracJourney);
                boatTrail.SetPosition(linePoints, boat.transform.position);
            }
        }

        Vector3 pos;
        for (int i = 0; i <= linePoints; i++)
        {
            pos = boatTrail.GetPosition(i);
            pos.y = boat.transform.position.y;
            boatTrail.SetPosition(i, pos);
        }
    }

    void OnBoatInTyphoonEvent(BoatInTyphoonEvent e)
    {
        Debug.Log("Boat in typhoon!");
        AkSoundEngine.PostEvent("Play_Typhon", gameObject);
        StartCoroutine(WaitBeforeGoingToInitialPos(e.typhoon));
    }

    IEnumerator WaitBeforeGoingToInitialPos(GameObject typhoon)
    {
        yield return new WaitForSeconds(0.5f);
        typhoon.GetComponent<SpriteRenderer>().enabled = true;
        fromTyphoon = true;
        LaunchNavigation(lastValidPosition.transform.position, lastValidPositionZone, false);
    }

    void OnBoatInMapElement(BoatInMapElement e)
    {
        Debug.Log("Magnetiiiiism");
        LaunchNavigation(e.newTarget, e.newMapZone, false);
    }

    public void Navigate()
    {
        LaunchNavigation(lastValidTarget, lastValidTargetZone, true);
    }

    void LaunchNavigation(Vector3 target, int newZoneNumber, bool beginJourney)
    {
        EventManager.Instance.Raise(new BlockInputEvent() { block = true, navigation = true });
        AkSoundEngine.PostEvent("Stop_SoundClue", gameObject);

        // Initialize navigation values
        navigating = true;
        currentSpeed = (fromTyphoon ? boatSpeedFromTyhpoon : boatSpeed);
        boatRenderer.sprite = boatSprites[0];
        lightScript.rotateDegreesPerSecond.value.y = sunMove;
        target.y = boat.transform.position.y;
        journeyLength = (target - boat.transform.position).sqrMagnitude;
        journeyTarget = target;
        journeyBeginTime = Time.time;

        // Update boat trail
        linePoints++;
        boatTrail.positionCount = linePoints + 1;
        boatTrail.SetPosition(linePoints, boat.transform.position);

        // Update map zone
        if (newZoneNumber != map.currentZone)
            map.ChangeZone(newZoneNumber);

        if (beginJourney)
        {
            AkSoundEngine.PostEvent("Play_Travel", gameObject);

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
            //Debug.Log("Going into a typhoon? " + goingIntoTyphoon);
        }
    }

    void BerthOnIsland(bool tutorial)
    {
        onIsland = true;
        boatRenderer.sprite = boatSprites[1];

        // Reset rotations
        boat.transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, 0);
        boat.transform.localRotation = Quaternion.Euler(90, 0, 0);

        StartCoroutine(screenManager.Berth(boatScript.currentIsland, tutorial));
    }

    public void UpdateNavigation(Vector3 targetPos)
    {
        targetPos.y = boat.transform.position.y;
        ENavigationResult result = GetNavigationResult(targetPos);
        switch (result)
        {
            case ENavigationResult.SEA:
                CursorManager.Instance.SetCursor(ECursor.NAVIGATION_OK);
                break;

            case ENavigationResult.ISLAND:
                CursorManager.Instance.SetCursor(ECursor.NAVIGATION_ISLAND);
                break;
        }
    }

    ENavigationResult GetNavigationResult(Vector3 targetPos)
    {
        insideGoal = false;
        Vector3 journey = targetPos - boat.transform.position;
        float distance = journey.magnitude;

        Vector3 rayOrigin = targetPos;
        rayOrigin.y += 1;
        RaycastHit[] hitsAtTarget = Physics.RaycastAll(rayOrigin, new Vector3(0, -1, 0), 5);

        if (goalCollider && hitsAtTarget.Any(hit => ReferenceEquals(hit.collider.gameObject, goalCollider.gameObject)))
            insideGoal = true;

        // Visible island at target position
        RaycastHit island = hitsAtTarget.FirstOrDefault(hit => hit.collider.GetComponent<Island>() && hit.collider.GetComponent<Island>().visible && hit.collider.GetComponent<Island>().islandNumber != screenManager.currentIslandNumber);
        float distanceToTarget = (island.point - boat.transform.position).sqrMagnitude;
        if (island.collider && distanceToTarget <= maxDistanceSqr)
        {
            lastValidCursorPos = targetPos;
            lastValidTarget = island.transform.position;
            lastValidTargetZone = island.collider.GetComponent<Island>().islandNumber;
            return ENavigationResult.ISLAND;
        }

        // Stone magnetism
        RaycastHit stone = hitsAtTarget.FirstOrDefault(hit => hit.collider.GetComponentInParent<MapElement>() && hit.collider.GetComponentInParent<Island>() == null);
        distanceToTarget = (stone.point - boat.transform.position).sqrMagnitude;
        if (stone.collider && distanceToTarget <= maxDistanceSqr)
        {
            lastValidCursorPos = targetPos;
            lastValidTarget = stone.transform.position;
            return ENavigationResult.SEA;
        }

        // Visible map zone
        RaycastHit mapZone = hitsAtTarget.FirstOrDefault(hit => hit.collider.GetComponent<MapZone>() && hit.collider.GetComponent<MapZone>().visible);
        if (mapZone.collider)
        {
            lastValidTargetZone = mapZone.collider.GetComponent<MapZone>().zoneNumber;
            distanceToTarget = (mapZone.point - boat.transform.position).sqrMagnitude;

            if (distanceToTarget <= maxDistanceSqr)
            {
                lastValidCursorPos = targetPos;
                lastValidTarget = targetPos;
                return ENavigationResult.SEA;
            }
            else
            {
                lastValidCursorPos = FindMaxDistanceOnTrajectory(journey, distance, targetPos);
                lastValidTarget = lastValidCursorPos;
                return ENavigationResult.SEA;
            }
        }
        else
        {
            // No map zone visible at target pos
            RaycastHit[] hitsOnReverseJourney = Physics.RaycastAll(targetPos, -journey, distance);
            hitsOnReverseJourney = hitsOnReverseJourney.OrderByDescending(hit => Vector3.SqrMagnitude(boat.transform.position - hit.point)).ToArray();
            mapZone = hitsOnReverseJourney.FirstOrDefault(hit => hit.collider.GetComponent<MapZone>() && hit.collider.GetComponent<MapZone>().visible);
            float distanceToBorder = (mapZone.point - boat.transform.position).sqrMagnitude;
            distanceToTarget = (targetPos - boat.transform.position).sqrMagnitude;
            if (distanceToTarget <= maxDistanceSqr || distanceToBorder <= maxDistanceSqr)
            {
                lastValidCursorPos = mapZone.point;
                lastValidTarget = mapZone.point;
                return ENavigationResult.SEA;
            }
            else
            {
                lastValidCursorPos = FindMaxDistanceOnTrajectory(journey, distance, targetPos);
                lastValidTarget = lastValidCursorPos;
                return ENavigationResult.SEA;
            }
        }
    }

    Vector3 FindMaxDistanceOnTrajectory(Vector3 journey, float distance, Vector3 targetPos)
    {
        float factor = maxDistance / distance;
        float x = (targetPos.x - boat.transform.position.x) * factor + boat.transform.position.x;
        float y = (targetPos.y - boat.transform.position.y) * factor + boat.transform.position.y;
        float z = (targetPos.z - boat.transform.position.z) * factor + boat.transform.position.z;
        return new Vector3(x, y, z);
    }
}
