﻿using System.Collections;
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
            // End of journey
            if (Vector3.Distance(boat.transform.position, journeyTarget) <= 0.1f)
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
                        ResetTelescopeAnimation();
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
                if (Vector3.Distance(boat.transform.position, journeyTarget) <= 1f && !hasPlayedAnim && (!islandTarget || islandTarget && !islandTarget.firstTimeVisiting) && !navigatingToTyphoon)
                {
                    hasPlayedAnim = true;
                    telescope.ResetZoom();
                    StartCoroutine(telescope.PlayAnimation(false, true, map.GetCurrentZoneSprite()));
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
        if (Vector3.Distance(targetPos, boat.transform.position) <= maxDistance)
        {
            Vector3 rayOrigin = targetPos;
            rayOrigin.y += 1;
            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, new Vector3(0, -1, 0), 5);

            if (hits.Any(hit => hit.collider.GetComponent<Island>() && hit.collider.GetComponent<Island>().visible))
                return ENavigationResult.ISLAND;
            else
            {
                Vector3 direction = (targetPos - boat.transform.position);
                float distance = direction.magnitude;
                direction.Normalize();
                hits = Physics.RaycastAll(boat.transform.position, direction, distance);
                if (hits.Any(hit => hit.collider.GetComponent<Island>() || (hit.collider.GetComponent<MapZone>() && !hit.collider.GetComponent<MapZone>().visible)))
                    return ENavigationResult.KO;
                if (hits.Any(hit => hit.collider.CompareTag("Typhoon")))
                    return ENavigationResult.TYPHOON;
                else
                    return ENavigationResult.SEA;
            }
        }

        return ENavigationResult.KO;
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
        journeyLength = Vector3.Distance(target, boat.transform.position);
        journeyTarget = target;
        journeyTarget.y = boatPosY;
        journeyBeginTime = Time.time;
        StartCoroutine(telescope.PlayAnimation(true, false));
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

    public void NavigateToTyphoon(Vector3 targetPos)
    {
        if (Vector3.Distance(targetPos, boat.transform.position) >= minDistance)
        {
            LaunchNavigation(targetPos);
            navigatingToTyphoon = true;
            initialPos = boat.transform.position;
        }
    }

    void ResetTelescopeAnimation()
    {
        telescope.ResetAnimation();
    }

    void OnBoatInTyphoonEvent(BoatInTyphoonEvent e)
    {
        journeyLength = Vector3.Distance(initialPos, boat.transform.position);
        journeyTarget = initialPos;
        journeyBeginTime = Time.time;
    }
}
