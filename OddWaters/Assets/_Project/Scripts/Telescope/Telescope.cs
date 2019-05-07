using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ELayer
{
    BACKGROUND,
    HORIZON,
    SEA,
    WATER3,
    WATER2,
    WATER1
};

public class Telescope : MonoBehaviour
{
    [SerializeField]
    Boat boat;

    SpriteRenderer[] spriteRenderers;

    // Drag
    [SerializeField]
    [Range(0.1f, 2)]
    float dragSpeedNormal = 1;
    [SerializeField]
    [Range(0.01f, 1)]
    float dragSpeedZoom = 0.3f;
    [SerializeField]
    Transform layersContainer;
    TelescopeLayer[] layers;
    float currentDragSpeed;
    float telescopePosMax;

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
    [SerializeField]
    GameObject colliderNormal;
    [SerializeField]
    GameObject colliderZoom;

    bool zoomAnimation;
    Vector3 scaleMaskTarget;
    Vector3 scaleContainerTarget;
    float zoomAnimationAlpha;

    [SerializeField]
    float elementDetectionSensitivity = 0.5f;

    void Start()
    {
        // Find layers
        layers = new TelescopeLayer[layersContainer.childCount];
        for (int i = 0; i < layersContainer.childCount; i++)
            layers[i] = layersContainer.GetChild(i).GetComponent<TelescopeLayer>();

        // Initialize drag values
        dragSpeedNormal /= 100f;
        dragSpeedZoom /= 100f;
        currentDragSpeed = 0;
        telescopePosMax = layers[(int)ELayer.BACKGROUND].layerSize / 2f;

        // Initialize zoom values
        zoom = false;
        wheelZoomLevel = 0;
        scaleMaskNormal = maskZoom.transform.localScale;
        scaleMaskZoom = new Vector3(1, 1, 1);
        scaleContainerNormal = new Vector3(1, 1, 1);
        scaleContainerZoom = new Vector3(zoomPower, zoomPower, zoomPower);       
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
        if ((wheelZoomLevel == 0 && zoom) || (wheelZoomLevel == wheelZoomThreshold && !zoom))
        {
            zoom = (zoomAmount > 0);
            SetZoom();
        }
    }

    void SetZoom()
    {
        zoomAnimation = true;
        zoomAnimationAlpha = 0;

        if (zoom)
        {
            colliderNormal.SetActive(false);
            colliderZoom.SetActive(true);
            scaleMaskTarget = scaleMaskZoom;
            scaleContainerTarget = scaleContainerZoom;
            AkSoundEngine.PostEvent("Play_Telescope_Open", gameObject);
        }
        else
        {
            colliderNormal.SetActive(true);
            colliderZoom.SetActive(false);
            scaleMaskTarget = scaleMaskNormal;
            scaleContainerTarget = scaleContainerNormal;
            AkSoundEngine.PostEvent("Play_Telescope_Close", gameObject);
        }
    }

    public void BeginDrag(Vector3 beginPos)
    {
        for (int i = 0; i < layers.Length; i++)
            layers[i].BeginDrag();
    }

    public void EndDrag()
    {
        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
        currentDragSpeed = 0;
        for (int i = 0; i < layers.Length; i++)
            layers[i].EndDrag();
    }

    public void UpdateSpeed(float speed)
    {
        currentDragSpeed = speed * (zoom ? dragSpeedZoom : dragSpeedNormal);

        for (int i = 0; i < layers.Length; i++)
            layers[i].dragSpeed = currentDragSpeed;

        if (currentDragSpeed == 0)
            CursorManager.Instance.SetCursor(ECursor.TELESCOPE_PAN_CENTER);
        else if (currentDragSpeed < 0)
            CursorManager.Instance.SetCursor(ECursor.TELESCOPE_PAN_RIGHT);
        else
            CursorManager.Instance.SetCursor(ECursor.TELESCOPE_PAN_LEFT);
    }

    void Update()
    {
        // Field of view rotation
        if (currentDragSpeed != 0)
        {
            float telescopeRotation = layers[(int)ELayer.BACKGROUND].children[0].localPosition.x * (360f / (telescopePosMax * 2)) - telescopePosMax * (360f / (telescopePosMax * 2)) + 180;
            boat.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0, 0, telescopeRotation);
        }
        // Element identification on zoom
        else
        {
            foreach (TelescopeElement element in layersContainer.GetComponentsInChildren<TelescopeElement>())
            {
                if (element.triggerActive)
                {
                    float distanceElement = Mathf.Abs(element.transform.position.x - maskZoom.transform.position.x);
                    if ((!element.needZoom && element.inSight) || (zoom && distanceElement <= elementDetectionSensitivity))
                        element.Trigger();
                }
            }
        }

        // Zoom animation
        if (zoomAnimation)
        {
            zoomAnimationAlpha += Time.deltaTime;
            maskZoom.transform.localScale = Vector3.Lerp(maskZoom.transform.localScale, scaleMaskTarget, zoomAnimationAlpha);
            layersContainer.transform.localScale = Vector3.Lerp(layersContainer.transform.localScale, scaleContainerTarget, zoomAnimationAlpha);

            if (zoomAnimationAlpha >= 1)
                zoomAnimation = false;
        }
    }

    void ResetPosition()
    {
        for (int i = 0; i < layers.Length; i++)
            layers[i].ResetPosition();
    }

    public void RefreshElements(Vector3 boatUp, Vector3 target, Vector3 boatRight, GameObject panorama)
    {
        if (panorama != null)
        {
            /*Destroy(telescopesPanoramaFolder[0]);
            Destroy(telescopesPanoramaFolder[1]);
            GameObject newPanorama1 = Instantiate(panorama, telescopes[0].transform);
            GameObject newPanorama2 = Instantiate(panorama, telescopes[1].transform);
            telescopesPanoramaFolder[0] = newPanorama1;
            telescopesPanoramaFolder[1] = newPanorama2;*/
        }

        ResetPosition();

        foreach (TelescopeElement element in layersContainer.GetComponentsInChildren<TelescopeElement>())
        {
            Destroy(element.gameObject);
        }

        foreach (MapElement element in boat.GetElementsInSight())
        {
            Island island = element.GetComponent<Island>();
            if (!island || island != boat.currentIsland)
            {
                // Create first telescope element
                GameObject telescopeElementObject1 = new GameObject(element.name);
                SpriteRenderer spriteRenderer = telescopeElementObject1.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = element.elementSprite;
                spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                BoxCollider collider = telescopeElementObject1.AddComponent<BoxCollider>();
                collider.isTrigger = true;

                if (element.layer == ELayer.HORIZON)
                    spriteRenderer.sortingOrder = 4;
                else
                    spriteRenderer.sortingOrder = 6;

                telescopeElementObject1.transform.parent = layers[(int)element.layer].children[0];
                telescopeElementObject1.transform.rotation = Quaternion.Euler(90, 0, 0);
                telescopeElementObject1.transform.localScale = new Vector3(1, 1, 1);

                TelescopeElement telescopeElement1 = telescopeElementObject1.AddComponent<TelescopeElement>();
                telescopeElement1.elementDiscover = element;
                telescopeElement1.triggerActive = !element.visible;
                telescopeElement1.needZoom = (island != null);

                // Clone it
                GameObject telescopeElementObject2 = Instantiate(telescopeElementObject1, layers[(int)element.layer].children[1]);

                // Place them on telescope in 0-360°
                float angle = Angle360(-boatUp, element.transform.position - target, boatRight);
                angle = 360 - angle;
                float layerWidth = layers[(int)element.layer].layerSize * layers[(int)element.layer].parallaxSpeed;
                float offset = angle * (layerWidth / 360f) - (layerWidth / 2f);
                telescopeElementObject1.transform.localPosition = new Vector3(offset, 0, 0);
                telescopeElementObject2.transform.localPosition = new Vector3(offset, 0, 0);

                // Initialize telescope elements
                TelescopeElement telescopeElement2 = telescopeElementObject2.GetComponent<TelescopeElement>();
                telescopeElement1.cloneElement = telescopeElement2;
                telescopeElement2.cloneElement = telescopeElement1;
                
                AkSoundEngine.PostEvent("Play_Clue_" + element.name, gameObject);
            }
        }
    }

    float Angle360(Vector3 from, Vector3 to, Vector3 right)
    {
        from.y = 0;
        to.y = 0;
        float angle = Vector3.Angle(from, to);
        return (Vector3.Angle(right, to) > 90f) ? 360f - angle : angle;
    }

    public void SetImageAlpha(bool dark)
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        float alpha = dark ? 0.3f : 1;
        Color color = new Color(alpha, alpha, alpha, 1);

        foreach (SpriteRenderer sprite in spriteRenderers)
            sprite.color = color;
    }
}
