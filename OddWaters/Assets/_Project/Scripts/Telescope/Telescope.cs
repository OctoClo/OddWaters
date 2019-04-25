using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ELayer
{
    BACKGROUND,
    HORIZON,
    SEA,
    FOREGROUND
};

public class Telescope : MonoBehaviour
{
    [SerializeField]
    Animator fadeAnimator;
    SpriteRenderer animationSprite;

    [SerializeField]
    Boat boat;

    // Drag
    [SerializeField]
    [Range(0.1f, 2)]
    float dragSpeedNormal = 1;
    [SerializeField]
    [Range(0.01f, 1)]
    float dragSpeedZoom = 0.3f;
    [SerializeField]
    Sprite backgroundSprite;
    [SerializeField]
    Transform layersContainer;
    TelescopeLayer[] layers;
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
    [SerializeField]
    GameObject colliderNormal;
    [SerializeField]
    GameObject colliderZoom;

    bool zoomAnimation;
    Vector3 scaleMaskTarget;
    Vector3 scaleContainerTarget;
    float zoomAnimationAlpha;

    [SerializeField]
    float islandDetectionSensitivity = 0.5f;
    GameObject islandInSight = null;

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
        cursorScale = new Vector3(1f, 1f, 0);
        telescopePosMax = layers[(int)ELayer.BACKGROUND].layerSize / 2f;

        // Initialize zoom values
        zoom = false;
        wheelZoomLevel = 0;
        scaleMaskNormal = maskZoom.transform.localScale;
        scaleMaskZoom = new Vector3(1, 1, 1);
        scaleContainerNormal = new Vector3(1, 1, 1);
        scaleContainerZoom = new Vector3(zoomPower, zoomPower, zoomPower);
        animationSprite = fadeAnimator.gameObject.GetComponent<SpriteRenderer>();
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

        if (zoom)
        {
            colliderNormal.SetActive(false);
            colliderZoom.SetActive(true);
            scaleMaskTarget = scaleMaskZoom;
            scaleContainerTarget = scaleContainerZoom;
        }
        else
        {
            colliderNormal.SetActive(true);
            colliderZoom.SetActive(false);
            scaleMaskTarget = scaleMaskNormal;
            scaleContainerTarget = scaleContainerNormal;
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
        else if (zoom)
        {
            foreach (TelescopeElement element in layersContainer.GetComponentsInChildren<TelescopeElement>())
            {
                float distanceElement = (element.transform.position - maskZoom.transform.position).sqrMagnitude;
                if (!element.islandDiscover.visible && element.gameObject.activeInHierarchy && distanceElement <= islandDetectionSensitivity * islandDetectionSensitivity)
                    element.Trigger();
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

        foreach (Island island in boat.GetIslandsInSight())
        {
            // Create islands
            GameObject island1 = new GameObject("Island" + island.islandNumber);
            SpriteRenderer spriteRenderer = island1.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = island.islandSprite;
            spriteRenderer.sortingOrder = 4;
            island1.AddComponent<BoxCollider>();
            island1.transform.parent = layers[(int)ELayer.HORIZON].children[0];
            island1.transform.rotation = Quaternion.Euler(90, 0, 0);
            island1.transform.localScale = new Vector3(1, 1, 1);

            GameObject island2 = new GameObject("Island" + island.islandNumber);
            spriteRenderer = island2.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = island.islandSprite;
            spriteRenderer.sortingOrder = 4;
            island2.AddComponent<BoxCollider>();
            island2.transform.parent = layers[(int)ELayer.HORIZON].children[1];
            island2.transform.rotation = Quaternion.Euler(90, 0, 0);
            island2.transform.localScale = new Vector3(1, 1, 1);

            // Place islands in 0-360°
            float angle = Angle360(-boatUp, island.transform.position - target, boatRight);
            angle = 360 - angle;
            float offset = angle * (backgroundSprite.texture.width / 360f) - (backgroundSprite.texture.width / 2f);
            offset /= backgroundSprite.pixelsPerUnit;
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
        fadeAnimator.Play("Base Layer.TelescopeLight");
    }

    public void SetImageAlpha(bool dark)
    {
        if (dark)
            fadeAnimator.Play("Base Layer.TelescopeDark");
        else
            fadeAnimator.Play("Base Layer.TelescopeLight");
    }

}
