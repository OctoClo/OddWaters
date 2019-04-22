using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Telescope : MonoBehaviour
{
    [SerializeField]
    Animator fadeAnimator;

    [SerializeField]
    Boat boat;

    // General
    [SerializeField]
    GameObject telescope1;
    
    GameObject[] telescopes;
    Transform telescopeContainer;
    GameObject completeZone1;
    GameObject completeZone2;
    float telescopeOffsetX;
    float spriteZoneWidth;
    float spriteZonePPU;
    
    // Drag
    [SerializeField]
    [Range(0.1f, 2)]
    float dragSpeedNormal = 1;
    [SerializeField]
    [Range(0.01f, 1)]
    float dragSpeedZoom = 0.3f;
    float currentDragSpeed;
    float telescopePosMax;
    Vector3 cursorScale;

    // Zoom
    bool zoom;
    [SerializeField]
    GameObject maskZoom;
    [SerializeField]
    float zoomPower = 1.5f;
    [SerializeField]
    [Tooltip("Augmente de 0.1 en 0.1")]
    float wheelZoomThreshold = 0.3f;
    float wheelZoomLevel;
    Vector3 scaleMaskNormal;
    Vector3 scaleMaskZoom;
    Vector3 scaleContainerNormal;
    Vector3 scaleContainerZoom;

    bool zoomAnimation;
    Vector3 scaleMaskTarget;
    Vector3 scaleContainerTarget;
    float zoomAnimationAlpha;

    [SerializeField]
    float islandDetectionSensitivity = 0.5f;
    GameObject islandInSight = null;

    void Start()
    {
        // Find some game objects and calculate things
        telescopeContainer = telescope1.transform.parent;
        completeZone1 = telescope1.transform.GetChild(0).gameObject;
        Sprite sprite = completeZone1.GetComponent<SpriteRenderer>().sprite;
        spriteZoneWidth = sprite.texture.width;
        spriteZonePPU = sprite.pixelsPerUnit;
        telescopeOffsetX = spriteZoneWidth * completeZone1.transform.localScale.x / spriteZonePPU;

        // Clone telescope 1
        GameObject telescope2 = Instantiate(telescope1, telescopeContainer);
        telescope2.name = "Telescope2";
        completeZone2 = telescope2.transform.GetChild(0).gameObject;
        Vector3 telescope2Pos = telescope1.transform.position;
        telescope2Pos.x = telescopeOffsetX;
        telescope2.transform.position = telescope2Pos;

        // Initialize telescope array
        telescopes = new GameObject[2];
        telescopes[0] = telescope1;
        telescopes[1] = telescope2;

        // Initialize drag values
        dragSpeedNormal /= 100f;
        dragSpeedZoom /= 100f;
        currentDragSpeed = 0;
        cursorScale = new Vector3(1f, 1f, 0);
        telescopePosMax = telescopeOffsetX / 2f;

        // Initialize zoom values
        zoom = false;
        wheelZoomLevel = 0;
        scaleMaskNormal = maskZoom.transform.localScale;
        scaleMaskZoom = new Vector3(1, 1, 1);
        scaleContainerNormal = new Vector3(1, 1, 1);
        scaleContainerZoom = new Vector3(zoomPower, zoomPower, zoomPower);
    }

    public void SetImageAlpha(bool dark)
    {
        float alpha = (dark ? 0.5f : 1);
        completeZone1.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
        completeZone2.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
    }

    public void ResetZoom()
    {
        wheelZoomLevel = 0;
        zoom = false;
        SetZoom();
    }

    public void Zoom(float zoomAmount)
    {
        wheelZoomLevel += zoomAmount;
        wheelZoomLevel = Mathf.Clamp(wheelZoomLevel, 0f, wheelZoomThreshold);
        if (wheelZoomLevel < 0.1f)
            wheelZoomLevel = 0;
        if (wheelZoomLevel == 0 || wheelZoomLevel == wheelZoomThreshold)
        {
            zoom = (zoomAmount > 0);
            SetZoom();
        }
    }

    void SetZoom()
    {
        zoomAnimation = true;
        zoomAnimationAlpha = 0;
        scaleMaskTarget = (zoom ? scaleMaskZoom : scaleMaskNormal);
        scaleContainerTarget = (zoom ? scaleContainerZoom : scaleContainerNormal);
    }

    public void BeginDrag(Vector3 beginPos)
    {

    }

    public void EndDrag()
    {
        currentDragSpeed = 0;
        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
    }

    public void UpdateSpeed(float speed)
    {
        currentDragSpeed = speed * (zoom ? dragSpeedZoom : dragSpeedNormal);
        if (currentDragSpeed == 0)
            CursorManager.Instance.SetCursor(ECursor.TELESCOPE_PAN_CENTER);
        else if (currentDragSpeed < 0)
            CursorManager.Instance.SetCursor(ECursor.TELESCOPE_PAN_RIGHT);
        else
            CursorManager.Instance.SetCursor(ECursor.TELESCOPE_PAN_LEFT);
    }

    void Update()
    {
        Vector3 move = new Vector3(currentDragSpeed * Time.deltaTime, 0, 0);
        telescopes[0].transform.position += move;
        telescopes[1].transform.position += move;

        // Swap telescopes if needed
        if (currentDragSpeed < 0 && telescopes[1].transform.position.x <= maskZoom.transform.position.x)
        {
            Vector3 newPos = telescopes[0].transform.position;
            newPos.x = telescopes[1].transform.position.x + telescopeOffsetX;
            telescopes[0].transform.position = newPos;
            SwapTelescopes();
        }
        else if (currentDragSpeed > 0 && telescopes[0].transform.position.x >= maskZoom.transform.position.x)
        {
            Vector3 newPos = telescopes[1].transform.position;
            newPos.x = telescopes[0].transform.position.x - telescopeOffsetX;
            telescopes[1].transform.position = newPos;
            SwapTelescopes();
        }

        // Field of view rotation
        if (currentDragSpeed != 0)
        {
            float telescopeRotation = telescopes[0].transform.localPosition.x * (360f / (telescopePosMax * 2)) - telescopePosMax * (360f / (telescopePosMax * 2)) + 180;
            boat.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0, 0, telescopeRotation);
        }
        // Element identification on zoom
        else if (zoom)
        {
            int islandCount = telescopes[0].transform.childCount - 1;
            for (int i = 1; i <= islandCount; i++)
            {
                TelescopeElement island3D1 = telescopes[0].transform.GetChild(i).GetComponent<TelescopeElement>();
                float distanceIsland1 = (island3D1.transform.position - maskZoom.transform.position).sqrMagnitude;
                if (!island3D1.islandDiscover.visible)
                {
                    if (island3D1.gameObject.activeInHierarchy && distanceIsland1 <= islandDetectionSensitivity * islandDetectionSensitivity)
                        island3D1.Trigger();

                    TelescopeElement island3D2 = telescopes[1].transform.GetChild(i).GetComponent<TelescopeElement>();
                    float distanceIsland2 = (island3D2.transform.position - maskZoom.transform.position).sqrMagnitude;
                    if (island3D2.gameObject.activeInHierarchy && distanceIsland2 <= islandDetectionSensitivity * islandDetectionSensitivity)
                        island3D2.Trigger();
                }
            }
        }

        // Zoom animation
        if (zoomAnimation)
        {
            zoomAnimationAlpha += Time.deltaTime;
            maskZoom.transform.localScale = Vector3.Lerp(maskZoom.transform.localScale, scaleMaskTarget, zoomAnimationAlpha);
            telescopeContainer.transform.localScale = Vector3.Lerp(telescopeContainer.transform.localScale, scaleContainerTarget, zoomAnimationAlpha);

            telescopeOffsetX = spriteZoneWidth * completeZone1.transform.localScale.x * telescopeContainer.localScale.x / spriteZonePPU;
            Vector3 telescope2Pos = telescopes[0].transform.position;
            telescope2Pos.x += telescopeOffsetX;
            telescopes[1].transform.position = telescope2Pos;


            if (zoomAnimationAlpha >= 1)
                zoomAnimation = false;
        }
    }

    void ResetPosition()
    {
        telescopes[0].transform.localPosition = Vector3.zero;
        Vector3 newPosition = telescopes[0].transform.position;
        newPosition.x += telescopeOffsetX;
        telescopes[1].transform.position = newPosition;
    }

    void SwapTelescopes()
    {
        GameObject temp = telescopes[0];
        telescopes[0] = telescopes[1];
        telescopes[1] = temp;
    }

    public void PlayAnimation(bool fadeIn, bool fadeOut)
    {
        if (fadeIn && fadeOut)
            fadeAnimator.Play("Base Layer.TelescopeFadeInOut");
        else
        {
            if (fadeIn)
                fadeAnimator.Play("Base Layer.TelescopeFadeIn");
            else if (fadeOut)
                fadeAnimator.Play("Base Layer.TelescopeFadeOut");
        }
    }

    public void RefreshElements(Vector3 boatUp, Vector3 target, Vector3 boatRight, Sprite sprite)
    {
        completeZone1.GetComponent<SpriteRenderer>().sprite = sprite;
        completeZone2.GetComponent<SpriteRenderer>().sprite = sprite;

        ResetPosition();

        int islandCount = telescopes[0].transform.childCount - 1;
        for (int i = 1; i <= islandCount; i++)
        {
            Destroy(telescopes[0].transform.GetChild(i).gameObject);
            Destroy(telescopes[1].transform.GetChild(i).gameObject);
        }

        foreach (Island island in boat.GetIslandsInSight())
        {
            // Create islands
            Vector3 spriteScale = completeZone1.transform.localScale;

            GameObject island1 = new GameObject("Island" + island.islandNumber);
            SpriteRenderer spriteRenderer = island1.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = island.islandSprite;
            spriteRenderer.sortingOrder = 2;
            island1.AddComponent<BoxCollider>();
            island1.transform.parent = telescopes[0].transform;
            island1.transform.rotation = Quaternion.Euler(90, 0, 0);
            island1.transform.localScale = spriteScale;

            GameObject island2 = new GameObject("Island" + island.islandNumber);
            spriteRenderer = island2.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = island.islandSprite;
            spriteRenderer.sortingOrder = 2;
            island2.AddComponent<BoxCollider>();
            island2.transform.parent = telescopes[1].transform;
            island2.transform.rotation = Quaternion.Euler(90, 0, 0);
            island2.transform.localScale = spriteScale;

            // Place islands in 0-360°
            float angle = Angle360(-boatUp, island.transform.position - target, boatRight);
            angle = 360 - angle;
            float offset = angle * (spriteZoneWidth / 360f) - (spriteZoneWidth / 2f);
            offset *= completeZone1.transform.localScale.x / spriteZonePPU;
            island1.transform.localPosition = new Vector3(offset, 0, 0);
            island2.transform.localPosition = new Vector3(offset, 0, 0);

            // Initialize telescope elements
            TelescopeElement island3D1Element = island1.AddComponent<TelescopeElement>();
            TelescopeElement island3D2Element = island2.AddComponent<TelescopeElement>();
            island3D1Element.cloneElement = island2;
            island3D2Element.cloneElement = island1;
            island3D1Element.islandDiscover = island;
            island3D2Element.islandDiscover = island;

            islandInSight = island.gameObject;
        }
    }

    float Angle360(Vector3 from, Vector3 to, Vector3 right)
    {
        from.y = 0;
        to.y = 0;
        float angle = Vector3.Angle(from, to);
        return (Vector3.Angle(right, to) > 90f) ? 360f - angle : angle;
    }

    public void ResetAnimation()
    {
        fadeAnimator.Play("Base Layer.Default");
    }
}
