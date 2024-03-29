using UnityEngine;

public enum ELayer
{
    BACKGROUND,
    HORIZON,
    SEA,
    WATER3,
    WATER2,
    WATER1,
    RAIN
};

public class Telescope : MonoBehaviour
{
    [Header("Drag")]
    [SerializeField]
    [Range(0.1f, 5)]
    float dragSpeedNormal = 1;
    [SerializeField]
    [Range(0.01f, 1)]
    float dragSpeedZoom = 0.3f;
    float currentDragSpeed;
    float telescopePosMax;
    [SerializeField]
    [Range(0.01f, 0.1f)]
    float dragMinValue = 0.06f;
    [SerializeField]
    [Range(0.1f, 0.5f)]
    float dragMaxValue = 0.25f;
    [SerializeField]
    Sprite indicatorDragSprite;
    GameObject indicatorDrag;

    [Header("Zoom")]
    [SerializeField]
    float zoomPower = 1.5f;
    [SerializeField]
    [Tooltip("Augmente de 0.1 en 0.1")]
    float wheelZoomThreshold = 0.3f;
    float wheelZoomLevel;
    bool zoom;
    Vector3 scaleMaskNormal;
    Vector3 scaleMaskZoom;
    Vector3 scaleContainerNormal;
    Vector3 scaleContainerZoom;
    [SerializeField]
    float elementDetectionSensitivity = 0.5f;

    [Header("References")]
    [SerializeField]
    TutorialManager tutorialManager;
    [HideInInspector]
    public bool tutorial;
    [SerializeField]
    GameObject maskZoom;
    [SerializeField]
    Boat boat;
    int boatRotation;
    [SerializeField]
    GameObject colliderNormal;
    [SerializeField]
    GameObject colliderZoom;
    [SerializeField]
    Transform layersContainer;
    TelescopeLayer[] layers;

    SpriteRenderer[] spriteRenderers;
    bool zoomAnimation;
    Vector3 scaleMaskTarget;
    Vector3 scaleContainerTarget;
    float zoomAnimationAlpha;

    RainManager rainManager;

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

        boat.transform.GetChild(0).gameObject.SetActive(false);
    }

    void OnEnable()
    {
        EventManager.Instance.AddListener<BoatAsksTelescopeRefreshEvent>(OnBoatAsksTelescopeRefreshEvent);
    }

    void OnDisable()
    {
        EventManager.Instance.RemoveListener<BoatAsksTelescopeRefreshEvent>(OnBoatAsksTelescopeRefreshEvent);
    }

    void ResetZoom()
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

    public void ChangeZoom()
    {
        zoom = !zoom;
        wheelZoomLevel = (zoom ? wheelZoomThreshold : 0);
        SetZoom();
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

        indicatorDrag = new GameObject("CursorBegin");
        indicatorDrag.transform.position = beginPos;
        indicatorDrag.transform.localRotation = Quaternion.Euler(90, 0, 0);
        indicatorDrag.transform.localScale = new Vector3(0.3f, 0.3f, 1);
        SpriteRenderer renderer = indicatorDrag.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 10;
        renderer.sprite = indicatorDragSprite;
        Color color = renderer.color;
        color.a = 0.5f;
        renderer.color = color;

        boat.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void EndDrag()
    {
        boat.transform.GetChild(0).gameObject.SetActive(false);
        Destroy(indicatorDrag);
        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
        currentDragSpeed = 0;
        for (int i = 0; i < layers.Length; i++)
            layers[i].EndDrag();
    }

    public void UpdateSpeed(float speed)
    {
        currentDragSpeed = speed * (zoom ? dragSpeedZoom : dragSpeedNormal);
        if (!zoom)
        {
            if (speed > 0)
                currentDragSpeed = Mathf.Clamp(currentDragSpeed, dragMinValue, dragMaxValue);
            else if (speed < 0)
                currentDragSpeed = Mathf.Clamp(currentDragSpeed, -dragMaxValue, -dragMinValue);
        }

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
            RefreshElementsAngle();
        // Element identification on zoom
        else
        {
            foreach (TelescopeElement element in layersContainer.GetComponentsInChildren<TelescopeElement>())
            {
                if (element.triggerActive)
                {
                    float distanceElement = Mathf.Abs(element.transform.position.x - maskZoom.transform.position.x);
                    if (!element.needSight
                        || (!element.needZoom && ((element.needSuperPrecision && distanceElement <= elementDetectionSensitivity)
                                                || (!element.needSuperPrecision && element.inSight)))
                        || (zoom && distanceElement <= elementDetectionSensitivity))
                    {
                        element.Trigger(tutorial, tutorialManager);
                    }
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

    public void StartNavigation()
    {
        ResetZoom();
        rainManager.UpdateRain(ERainType.NONE);
    }

    public void RefreshElements(Vector3 target, GameObject panorama, ERainType rain)
    {
        // Update panorama
        foreach (Transform child in layersContainer)
            Destroy(child.gameObject);

        GameObject layer = null;
        for (int i = 0; i < panorama.transform.childCount; i++)
        {
            layer = Instantiate(panorama.transform.GetChild(i).gameObject, layersContainer);
            layers[i] = layer.GetComponent<TelescopeLayer>();
        }

        rainManager = layer.GetComponent<RainManager>();
        rainManager.UpdateRain(rain);

        ResetPosition();

        // Spwan map elements in sight
        foreach (MapElement element in boat.GetElementsInSight())
        {
            Island island = element.GetComponent<Island>();
            if (!island || island != boat.currentIsland)
                AddElementToPanorama(element, target);
        }

        RefreshElementsAngle();
    }

    void RefreshElementsAngle()
    {
        float telescopeRotation = layers[(int)ELayer.BACKGROUND].children[0].localPosition.x * (360f / (telescopePosMax * 2)) - telescopePosMax * (360f / (telescopePosMax * 2)) + 180;
        boat.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0, 0, telescopeRotation);
        boatRotation = Mathf.RoundToInt(telescopeRotation + 360);
        foreach (TelescopeElement element in layersContainer.GetComponentsInChildren<TelescopeElement>())
            element.angleToBoat = (element.startAngle + boatRotation) % 360;
    }

    void AddElementToPanorama(MapElement element, Vector3 target)
    {
        // Create first telescope element
        GameObject telescopeElementObject1 = new GameObject(element.name);
        SpriteRenderer spriteRenderer = telescopeElementObject1.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = element.elementSprite;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        BoxCollider collider = telescopeElementObject1.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        if (element.layer == ELayer.HORIZON)
            spriteRenderer.sortingOrder = 3;
        else
            spriteRenderer.sortingOrder = 6;

        telescopeElementObject1.transform.parent = layers[(int)element.layer].children[0];
        telescopeElementObject1.transform.rotation = Quaternion.Euler(90, 0, 0);
        telescopeElementObject1.transform.localScale = new Vector3(1, 1, 1);

        TelescopeElement telescopeElement1 = telescopeElementObject1.AddComponent<TelescopeElement>();
        telescopeElement1.elementDiscover = element;
        telescopeElement1.triggerActive = !element.visible;
        telescopeElement1.needZoom = element.needZoom;
        telescopeElement1.needSight = element.needSight;
        telescopeElement1.needSuperPrecision = element.needSuperPrecision;
        if (element.clueOneShot)
        {
            if (!element.clueAlreadyPlayed)
            {
                telescopeElement1.playClue = true;
                element.clueAlreadyPlayed = true;
            }
            else
                telescopeElement1.playClue = false;
        }
        else
            telescopeElement1.playClue = element.playClue;

        // Clone it
        GameObject telescopeElementObject2 = Instantiate(telescopeElementObject1, layers[(int)element.layer].children[1]);

        // Place them on telescope in 0-360°
        if (target == Vector3.zero)
            target = boat.transform.position;
        float angle = Angle360(-boat.transform.up, element.transform.position - target, boat.transform.right);
        angle = 360 - angle;
        float layerWidth = layers[(int)element.layer].layerSize * layers[(int)element.layer].parallaxSpeed;
        float offset = angle * (layerWidth / 360f) - (layerWidth / 2f);
        telescopeElementObject1.transform.localPosition = new Vector3(offset, 0, 0);
        telescopeElementObject2.transform.localPosition = new Vector3(offset, 0, 0);

        // Initialize telescope elements
        TelescopeElement telescopeElement2 = telescopeElementObject2.GetComponent<TelescopeElement>();
        telescopeElement1.cloneElement = telescopeElement2;
        telescopeElement2.cloneElement = telescopeElement1;
        int elementAngle = Mathf.RoundToInt(angle + 180) % 360;
        telescopeElement1.startAngle = elementAngle;
        telescopeElement2.startAngle = elementAngle;
        int elementCurrentAngle = (elementAngle + boatRotation) % 360;
        telescopeElement1.angleToBoat = elementCurrentAngle;
        telescopeElement2.angleToBoat = elementCurrentAngle;
        telescopeElement1.audio = true;
    }

    void OnBoatAsksTelescopeRefreshEvent(BoatAsksTelescopeRefreshEvent e)
    {
        AddElementToPanorama(e.element, Vector3.zero);
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
        float colorChange = dark ? -0.4f : 0.4f;
        Color color;

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sprite in spriteRenderers)
        {
            color = sprite.color;
            color.r += colorChange;
            color.g += colorChange;
            color.b += colorChange;
            sprite.color = color;
        }
    }
}
