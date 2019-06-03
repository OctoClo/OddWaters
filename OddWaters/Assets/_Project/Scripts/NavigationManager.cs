using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Aura2API;

public class GameFinishedEvent : GameEvent { };

public enum ENavigationResult
{
    SEA,
    ISLAND,
    KO
}

public class NavigationManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField]
    float boatSpeed;
    [SerializeField]
    float boatSpeedFromTyhpoon;
    [SerializeField]
    float boatSpeedEnd;
    float currentSpeed;
    [SerializeField]
    float maxDistance = 3f;
    float maxDistanceSqr;
    [SerializeField]
    Material[] boatMaterials;
    [SerializeField]
    int sunMove = -3;    

    [Header("References")]
    [SerializeField]
    ScreenManager screenManager;
    [SerializeField]
    Telescope telescope;
    [SerializeField]
    Map map;
    [SerializeField]
    AutoMoveAndRotate lightScript;
    [SerializeField]
    GameObject boat;
    Boat boatScript;
    Renderer boatRenderer;
    [SerializeField]
    TutorialManager tutorialManager;
    [HideInInspector]
    public Collider goalCollider;
    bool insideGoal;
    [SerializeField]
    Transform megaTyphoon;
    bool lastZone = false;

    bool navigating;
    Vector3 journeyTarget;
    float journeyBeginTime;
    float journeyLength;
    GameObject lastValidPosition;
    int lastValidPositionZone;
    bool onIsland = false;
    bool fromTyphoon = false;

    [HideInInspector]
    public Vector3 lastValidCursorPos;
    Vector3 lastValidTarget;
    int lastValidTargetZone;

    void Start()
    {
        boatRenderer = boat.GetComponent<Renderer>();
        boatScript = boat.GetComponent<Boat>();
        navigating = false;
        insideGoal = false;

        lastValidPosition = new GameObject("BoatGhost");
        lastValidPosition.transform.parent = map.gameObject.transform;
        lastValidPosition.transform.position = boat.transform.position;

        maxDistanceSqr = maxDistance * maxDistance;
        boatScript.lineMaxLenght = maxDistanceSqr;
    }

    void OnEnable()
    {
        EventManager.Instance.AddListener<BoatInTyphoonEvent>(OnBoatInTyphoonEvent);    
        EventManager.Instance.AddListener<BoatInMapElementEvent>(OnBoatInMapElement);    
        EventManager.Instance.AddListener<DiscoverZoneEvent>(OnDiscoverZoneEvent);    
    }

    void OnDisable()
    {
        EventManager.Instance.RemoveListener<BoatInTyphoonEvent>(OnBoatInTyphoonEvent);
        EventManager.Instance.RemoveListener<BoatInMapElementEvent>(OnBoatInMapElement);
        EventManager.Instance.RemoveListener<DiscoverZoneEvent>(OnDiscoverZoneEvent);
    }

    public IEnumerator InitializeTelescopeElements()
    {
        yield return new WaitForSeconds(0.1f);
        telescope.RefreshElements(boat.transform.position, map.GetCurrentPanorama());
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
                if (!boatScript.inATyphoon && !lastZone)
                {
                    AkSoundEngine.PostEvent("Play_Arrival", gameObject);
                    AkSoundEngine.PostEvent("Stop_Typhon", gameObject);
                    lightScript.rotateDegreesPerSecond.value.y = 0;
                    telescope.RefreshElements(journeyTarget, map.GetCurrentPanorama());

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
                        EventManager.Instance.Raise(new BlockInputEvent() { block = false, navigation = false });

                        if (goalCollider && insideGoal)
                            tutorialManager.CompleteStep();
                    }

                    fromTyphoon = false;
                    if (goalCollider && insideGoal)
                    {
                        goalCollider = null;
                        insideGoal = false;
                    }
                }
                else if (lastZone)
                    lightScript.rotateDegreesPerSecond.value.y = 0;
            }
            // Still journeying
            else
            {
                float distCovered = (Time.time - journeyBeginTime) * currentSpeed;
                float fracJourney = distCovered / journeyLength;
                boat.transform.position = Vector3.Lerp(boat.transform.position, journeyTarget, fracJourney);
            }
        }
    }

    void OnBoatInTyphoonEvent(BoatInTyphoonEvent e)
    {
        if (e.safe)
            e.typhoon.GetComponent<Renderer>().enabled = true;
        else
        {
            Debug.Log("Boat in typhoon!");
            AkSoundEngine.PostEvent("Play_Typhon", gameObject);
            StartCoroutine(WaitBeforeGoingToInitialPos(e.typhoon));
        }
    }

    IEnumerator WaitBeforeGoingToInitialPos(GameObject typhoon)
    {
        yield return new WaitForSeconds(0.5f);
        typhoon.GetComponent<Renderer>().enabled = true;
        fromTyphoon = true;
        LaunchNavigation(lastValidPosition.transform.position, lastValidPositionZone, false);
    }

    void OnBoatInMapElement(BoatInMapElementEvent e)
    {
        if (!e.exit)
            LaunchNavigation(e.elementZone.transform.position, e.elementZone.zone, false);
    }

    public void Navigate()
    {
        LaunchNavigation(lastValidTarget, lastValidTargetZone, true);
    }

    void LaunchNavigation(Vector3 target, int newZoneNumber, bool beginJourney)
    {
        if (lastZone)
            EventManager.Instance.Raise(new GameFinishedEvent());
        EventManager.Instance.Raise(new BlockInputEvent() { block = true, navigation = true });
        AkSoundEngine.PostEvent("Stop_SoundClue", gameObject);

        // Initialize navigation values
        navigating = true;
        currentSpeed = lastZone ? boatSpeedEnd : (fromTyphoon ? boatSpeedFromTyhpoon : boatSpeed);
        boatRenderer.material = boatMaterials[0];
        lightScript.rotateDegreesPerSecond.value.y = sunMove;
        target.y = boat.transform.position.y;
        journeyLength = (target - boat.transform.position).sqrMagnitude;
        journeyTarget = target;
        journeyBeginTime = Time.time;

        // Update boat trail
        boatScript.AddTrailPos();

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
        }
    }

    void BerthOnIsland(bool tutorial)
    {
        onIsland = true;
        boatRenderer.material = boatMaterials[1];

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
        if (!lastZone)
        {
            Vector3 journey = targetPos - boat.transform.position;
            float distance = journey.magnitude;
            float clampedDistance = distance;
            if (clampedDistance > maxDistance)
            {
                targetPos = FindMaxDistanceOnTrajectory(clampedDistance, targetPos);
                journey = targetPos - boat.transform.position;
                clampedDistance = maxDistance;
            }

            insideGoal = false;
            RaycastHit[] hitsOnJourney = Physics.RaycastAll(boat.transform.position, journey, clampedDistance);

            // Invisible map zone on trajectory?
            RaycastHit hitInfo = hitsOnJourney.FirstOrDefault(hit => hit.collider.GetComponent<MapZone>() && !hit.collider.GetComponent<MapZone>().visible);
            if (hitInfo.collider)
            {
                lastValidCursorPos = hitInfo.point;
                lastValidTarget = hitInfo.point;
                lastValidTargetZone = hitInfo.collider.GetComponent<MapZone>().zoneNumber;
                return ENavigationResult.SEA;
            }
            else
            {
                Vector3 rayOrigin = targetPos;
                rayOrigin.y += 1;
                RaycastHit[] hitsAtTarget = Physics.RaycastAll(rayOrigin, new Vector3(0, -1, 0), 5);

                // No map zone at target pos?
                if (!hitsAtTarget.Any(hit => hit.collider.GetComponent<MapZone>()))
                {
                    hitsOnJourney = Physics.RaycastAll(targetPos, -journey, distance);
                    hitInfo = hitsOnJourney.FirstOrDefault(hit => hit.collider.GetComponent<MapZone>() && hit.collider.GetComponent<MapZone>().zoneNumber != map.currentZone && hit.collider.GetComponent<MapZone>().visible);
                    if (!hitInfo.collider)
                        hitInfo = hitsOnJourney.FirstOrDefault(hit => hit.collider.GetComponent<MapZone>() && hit.collider.GetComponent<MapZone>().zoneNumber == map.currentZone);
                    lastValidCursorPos = hitInfo.point;
                    lastValidTarget = hitInfo.point;
                    lastValidTargetZone = hitInfo.collider.GetComponent<MapZone>().zoneNumber;
                    return ENavigationResult.SEA;
                }
                else
                {
                    // Inside tutorial goal?
                    if (goalCollider && hitsAtTarget.Any(hit => ReferenceEquals(hit.collider.gameObject, goalCollider.gameObject)))
                        insideGoal = true;

                    // Visible island at target pos?
                    hitInfo = hitsAtTarget.FirstOrDefault(hit => hit.collider.GetComponent<Island>() && hit.collider.GetComponent<Island>().visible && hit.collider.GetComponent<Island>().islandNumber != screenManager.currentIslandNumber);
                    if (hitInfo.collider)
                    {
                        lastValidCursorPos = targetPos;
                        lastValidTarget = hitInfo.transform.position;
                        lastValidTargetZone = hitInfo.collider.GetComponent<Island>().islandNumber;
                        return ENavigationResult.ISLAND;
                    }
                    else
                    {
                        // MapElement magnetism?
                        hitInfo = hitsAtTarget.FirstOrDefault(hit => hit.collider.GetComponentInParent<MapElement>() && hit.collider.GetComponentInParent<MapElement>().magnetism &&
                                                                     hit.collider.GetComponentInParent<Island>() == null);
                        if (hitInfo.collider)
                        {
                            lastValidCursorPos = targetPos;
                            lastValidTarget = hitInfo.transform.position;
                            lastValidTargetZone = hitInfo.collider.GetComponentInParent<MapZone>().zoneNumber;
                            return ENavigationResult.SEA;
                        }
                        else
                        {
                            // Sea
                            lastValidCursorPos = targetPos;
                            lastValidTarget = targetPos;
                            lastValidTargetZone = hitsAtTarget.FirstOrDefault(hit => hit.collider.GetComponent<MapZone>()).collider.GetComponent<MapZone>().zoneNumber;
                            return ENavigationResult.SEA;
                        }
                    }
                }
            }
        }
        else
        {
            Vector3 journey = megaTyphoon.position - boat.transform.position;
            journey.Normalize();
            float distance = (targetPos - boat.transform.position).magnitude;
            distance = Mathf.Min(distance, maxDistance);
            lastValidCursorPos = boat.transform.position + journey * distance;
            lastValidTarget = megaTyphoon.position;
            lastValidTargetZone = 4;
            return ENavigationResult.SEA;
        }
    }

    Vector3 FindMaxDistanceOnTrajectory(float distance, Vector3 targetPos)
    {
        float factor = maxDistance / distance;
        float x = (targetPos.x - boat.transform.position.x) * factor + boat.transform.position.x;
        float y = (targetPos.y - boat.transform.position.y) * factor + boat.transform.position.y;
        float z = (targetPos.z - boat.transform.position.z) * factor + boat.transform.position.z;
        return new Vector3(x, y, z);
    }

    void OnDiscoverZoneEvent(DiscoverZoneEvent e)
    {
        if (e.zoneNumber == 4)
            lastZone = true;
    }
}
