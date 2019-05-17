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
    float maxDistance = 3f;
    float minDistance = 0.3f;
    [SerializeField]
    LineRenderer boatTrail;
    int linePoints = 0;

    [SerializeField]
    Transform typhoonIconsFolder;
    [SerializeField]
    GameObject typhoonIconPrefab;

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

    [HideInInspector]
    public Vector3 lastValidTarget;
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
    }

    void OnEnable()
    {
        EventManager.Instance.AddListener<BoatInTyphoonEvent>(OnBoatInTyphoonEvent);    
    }

    void OnDisable()
    {
        EventManager.Instance.RemoveListener<BoatInTyphoonEvent>(OnBoatInTyphoonEvent);
    }

    public void InitializeTelescopeElements()
    {
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
                    AkSoundEngine.SetState("SeaIntensity", "CalmSea");
                    lightScript.rotateDegreesPerSecond.value.y = 0;

                    telescope.RefreshElements(boat.transform.up, journeyTarget, boat.transform.right, map.GetCurrentPanorama());

                    // Save position
                    lastValidPosition.transform.position = boat.transform.position;
                    lastValidPositionZone = map.currentZone;

                    // Berth on island if needed
                    if (boatScript.onAnIsland && boatScript.currentIsland.islandNumber != screenManager.currentIslandNumber)
                        BerthOnIsland((goalCollider != null));
                    else
                    {
                        //EndJourneyAtSea
                        screenManager.EndNavigationAtSea();
                        EventManager.Instance.Raise(new BlockInputEvent() { block = false });

                        if (goalCollider && insideGoal)
                            tutorialManager.NextStep();
                    }

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
                float distCovered = (Time.time - journeyBeginTime) * boatSpeed;
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

    public void Navigate()
    {
        Vector3 journey = lastValidTarget - boat.transform.position;
        if (journey.sqrMagnitude >= minDistance * minDistance)
        {
            LaunchNavigation(lastValidTarget, lastValidTargetZone);
        }
    }

    void OnBoatInTyphoonEvent(BoatInTyphoonEvent e)
    {
        Debug.Log("Boat in typhoon!");
        AkSoundEngine.SetState("SeaIntensity", "RoughSea");
        StartCoroutine(WaitBeforeGoingToInitialPos());
    }

    IEnumerator WaitBeforeGoingToInitialPos()
    {
        yield return new WaitForSeconds(1.5f);

        // Create typhoon icon
        GameObject typhoonIcon = Instantiate(typhoonIconPrefab, typhoonIconsFolder);
        typhoonIcon.transform.position = boat.transform.position;

        LaunchNavigation(lastValidPosition.transform.position, lastValidPositionZone, true);
    }

    void LaunchNavigation(Vector3 target, int newZoneNumber, bool fromTyphoon = false)
    {
        EventManager.Instance.Raise(new BlockInputEvent() { block = true });
        AkSoundEngine.PostEvent("Stop_SoundClue", gameObject);

        // Initialize navigation values
        navigating = true;
        boatRenderer.sprite = boatSprites[0];
        boat.transform.GetChild(0).gameObject.SetActive(true);
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

    void BerthOnIsland(bool tutorial)
    {
        onIsland = true;
        boatRenderer.sprite = boatSprites[1];

        // Reset rotations
        boat.transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, 0);
        boat.transform.GetChild(0).gameObject.SetActive(false);
        boat.transform.localRotation = Quaternion.Euler(90, 0, 0);

        StartCoroutine(screenManager.Berth(boatScript.currentIsland, tutorial));
    }

    public void UpdateNavigation(Vector3 targetPos)
    {
        targetPos.y += boat.transform.localPosition.y;
        ENavigationResult result = GetNavigationResult(targetPos);
        switch (result)
        {
            case ENavigationResult.SEA:
                CursorManager.Instance.SetCursor(ECursor.NAVIGATION_OK);
                break;

            case ENavigationResult.ISLAND:
                CursorManager.Instance.SetCursor(ECursor.NAVIGATION_ISLAND);
                break;

            /*case ENavigationResult.KO:
                CursorManager.Instance.SetCursor(ECursor.NAVIGATION_KO);
                break;*/
        }
    }

    ENavigationResult GetNavigationResult(Vector3 targetPos)
    {
        Vector3 journey = targetPos - boat.transform.position;
        float distance = journey.magnitude;

        Vector3 rayOrigin = targetPos;
        rayOrigin.y += 1;
        RaycastHit[] hitsAtTarget = Physics.RaycastAll(rayOrigin, new Vector3(0, -1, 0), 5);

        if (goalCollider && hitsAtTarget.Any(hit => ReferenceEquals(hit.collider.gameObject, goalCollider.gameObject)))
            insideGoal = true;

        // Visible island at target position or on trajectory (ok)
        RaycastHit island = hitsAtTarget.FirstOrDefault(hit => hit.collider.GetComponent<Island>() && hit.collider.GetComponent<Island>().visible && hit.collider.GetComponent<Island>().islandNumber != screenManager.currentIslandNumber);
        if (!island.collider)
        {
            RaycastHit[] hitsOnJourney = Physics.RaycastAll(boat.transform.position, journey, distance);
            island = hitsOnJourney.FirstOrDefault(hit => hit.collider.GetComponent<Island>() && hit.collider.GetComponent<Island>().visible && hit.collider.GetComponent<Island>().islandNumber != screenManager.currentIslandNumber);
        }  
        float distanceToTarget = (island.point - boat.transform.position).sqrMagnitude;
        if (island.collider && distanceToTarget <= maxDistance * maxDistance)
        {
            lastValidTarget = island.point;
            lastValidTargetZone = island.collider.GetComponent<Island>().islandNumber;
            return ENavigationResult.ISLAND;
        }

        // Visible map zone
        RaycastHit mapZone = hitsAtTarget.FirstOrDefault(hit => hit.collider.GetComponent<MapZone>() && hit.collider.GetComponent<MapZone>().visible);
        if (mapZone.collider)
        {
            lastValidTargetZone = mapZone.collider.GetComponent<MapZone>().zoneNumber;
            distanceToTarget = (mapZone.point - boat.transform.position).sqrMagnitude;

            if (distanceToTarget <= maxDistance * maxDistance)
            {
                lastValidTarget = targetPos;
                return ENavigationResult.SEA;
            }
            else
            {
                lastValidTarget = FindMaxDistanceOnTrajectory(journey, targetPos);
                return ENavigationResult.SEA;
            }
        }
        else
        {
            distanceToTarget = (targetPos - boat.transform.position).sqrMagnitude;
            RaycastHit[] hitsOnReverseJourney = Physics.RaycastAll(targetPos, -journey, distance);
            mapZone = hitsOnReverseJourney.FirstOrDefault(hit => hit.collider.GetComponent<MapZone>() && hit.collider.GetComponent<MapZone>().zoneNumber == lastValidPositionZone);
            float distanceToBorder = (mapZone.point - boat.transform.position).sqrMagnitude;
            if (distanceToTarget <= maxDistance * maxDistance || maxDistance > distanceToBorder)
            {
                lastValidTarget = mapZone.point;
                return ENavigationResult.SEA;
            }
            else
            {
                lastValidTarget = FindMaxDistanceOnTrajectory(journey, targetPos);
                return ENavigationResult.SEA;
            }
        }
    }

    Vector3 FindMaxDistanceOnTrajectory(Vector3 journey, Vector3 targetPos)
    {
        float factor = maxDistance / journey.magnitude;
        float x = (targetPos.x - boat.transform.position.x) * factor + boat.transform.position.x;
        float y = (targetPos.y - boat.transform.position.y) * factor + boat.transform.position.y;
        float z = (targetPos.z - boat.transform.position.z) * factor + boat.transform.position.z;
        return new Vector3(x, y, z);
    }
}
