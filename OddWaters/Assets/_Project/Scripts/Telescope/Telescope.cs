using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Telescope : MonoBehaviour
{
    [SerializeField]
    Sprite cursorCenter;
    [SerializeField]
    Sprite cursorLeft;
    [SerializeField]
    Sprite cursorRight;
    
    GameObject cursorBegin;
    Vector2 cursorOffset;
    Vector3 cursorScale;
    
    [SerializeField]
    GameObject telescope1;
    GameObject telescope2;
    Transform telescopeParent;
    Transform telescopeMask;
    GameObject[] telescopes;
    GameObject completeZone1;
    GameObject completeZone2;
    Sprite sprite1;
    Sprite sprite2;
    float telescopeOffsetX;

    bool dragInitialized;
    [SerializeField]
    float dragSpeedMultiplier = 0.01f;
    [SerializeField]
    float dragSpeedMultiplierZoom = 0.003f;
    float dragSpeed;

    [SerializeField]
    float scaleZoom = 1.5f;
    [SerializeField]
    [Tooltip("Augmente de 0.1 en 0.1")]
    float zoomLevelMax = 0.3f;
    float zoomLevel;
    Vector3 scaleParentZoom;
    Vector3 scaleParentNormal;
    Vector3 scaleMaskZoom;
    Vector3 scaleMaskNormal;
    bool zoom;

    [SerializeField]
    float detectionSensitivity = 0.5f;
    int telescopeElementsCount;
    TelescopeElement[] telescopeElements;

    [SerializeField]
    Animator fadeAnimator;

    void Start()
    {
        cursorOffset = new Vector2(cursorCenter.texture.width / 2, cursorCenter.texture.height / 2);
        cursorScale = new Vector3(1.5f, 1.5f, 0);

        completeZone1 = telescope1.transform.GetChild(0).gameObject;
        sprite1 = completeZone1.GetComponent<SpriteRenderer>().sprite;
        telescopeParent = telescope1.transform.parent;
        telescopeMask = telescopeParent.parent;
        telescopeOffsetX = sprite1.texture.width * completeZone1.transform.localScale.x * telescope1.transform.localScale.x * telescopeParent.localScale.x * telescopeMask.localScale.x / sprite1.pixelsPerUnit;

        telescope2 = Instantiate(telescope1, telescopeParent);
        telescope2.name = "Telescope2";
        completeZone2 = telescope2.transform.GetChild(0).gameObject;

        Vector3 telescope2Pos = telescope1.transform.position;
        telescope2Pos.x = telescopeOffsetX;
        telescope2.transform.position = telescope2Pos;

        telescopes = new GameObject[2];
        telescopes[0] = telescope1;
        telescopes[1] = telescope2;

        GameObject elementsFolder1 = telescope1.transform.GetChild(1).gameObject;
        GameObject elementsFolder2 = telescope2.transform.GetChild(1).gameObject;
        GameObject element1, element2;

        telescopeElementsCount = elementsFolder1.transform.childCount;
        telescopeElements = new TelescopeElement[telescopeElementsCount * 2];
        for (int i = 0; i < telescopeElementsCount; i++)
        {
            element1 = elementsFolder1.transform.GetChild(i).gameObject;
            element2 = elementsFolder2.transform.GetChild(i).gameObject;
            telescopeElements[i * i] = element1.GetComponent<TelescopeElement>();
            telescopeElements[i * i + 1] = element2.GetComponent<TelescopeElement>();
            telescopeElements[i * i].cloneElement = element2;
            telescopeElements[i * i + 1].cloneElement = element1;
        }

        dragSpeed = 0;
        dragInitialized = false;

        zoom = false;
        zoomLevel = 0;
        scaleMaskNormal = telescopeMask.localScale;
        scaleMaskZoom = telescopeMask.localScale;
        scaleMaskZoom.x /= 2;
        scaleParentNormal = telescopeParent.localScale;
        scaleParentZoom = telescopeParent.localScale;
        scaleParentZoom.x *= 2 * telescopeParent.localScale.x * scaleZoom;
        scaleParentZoom.y *= telescopeParent.localScale.y * scaleZoom;
    }

    public void SetImageAlpha(bool dark)
    {
        float alpha = (dark ? 0.5f : 1);
        completeZone1.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
        completeZone2.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
    }

    public void ResetZoom()
    {
        zoomLevel = 0;
        zoom = false;
        SetZoom();
    }

    public void Zoom(float zoomAmount)
    {
        zoomLevel += zoomAmount;
        zoomLevel = Mathf.Clamp(zoomLevel, 0f, zoomLevelMax);
        if (zoomLevel < 0.1f)
            zoomLevel = 0;
        if (zoomLevel == 0 || zoomLevel == zoomLevelMax)
        {
            zoom = (zoomAmount > 0);
            SetZoom();
        }
    }

    void SetZoom()
    {
        telescopeMask.localScale = (zoom ? scaleMaskZoom : scaleMaskNormal);
        telescopeParent.localScale = (zoom ? scaleParentZoom : scaleParentNormal);

        telescopeOffsetX = sprite1.texture.width * completeZone1.transform.localScale.x * telescope1.transform.localScale.x * telescopeParent.localScale.x * telescopeParent.parent.localScale.x / sprite1.pixelsPerUnit;
        Vector3 telescope2Pos = telescopes[0].transform.position;
        telescope2Pos.x += telescopeOffsetX;
        telescopes[1].transform.position = telescope2Pos;
    }

    public void BeginDrag(Vector3 beginPos)
    {
        if (!dragInitialized)
        {
            cursorBegin = new GameObject("CursorBegin");
            beginPos.y = 0;
            cursorBegin.transform.position = beginPos;
            cursorBegin.transform.rotation = Quaternion.Euler(90, 0, 0);
            cursorBegin.transform.localScale = cursorScale;
            SpriteRenderer renderer = cursorBegin.AddComponent<SpriteRenderer>();
            renderer.sprite = cursorCenter;
            renderer.sortingOrder = 2;
            dragInitialized = true;
        }
    }

    public void EndDrag()
    {
        dragSpeed = 0;
        Destroy(cursorBegin);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        dragInitialized = false;
    }

    public void UpdateSpeed(float speed)
    {
        dragSpeed = speed * (zoom ? dragSpeedMultiplierZoom : dragSpeedMultiplier);
        if (dragSpeed == 0)
            Cursor.SetCursor(cursorCenter.texture, cursorOffset, CursorMode.Auto);
        else if (dragSpeed < 0)
            Cursor.SetCursor(cursorRight.texture, cursorOffset, CursorMode.Auto);
        else
            Cursor.SetCursor(cursorLeft.texture, cursorOffset, CursorMode.Auto);
    }

    void Update()
    {
        Vector3 move = new Vector3(dragSpeed * Time.deltaTime, 0, 0);
        telescope1.transform.position += move;
        telescope2.transform.position += move;
        
        if (dragSpeed < 0 && telescopes[1].transform.position.x <= 0)
        {
            Vector3 newPos = telescopes[0].transform.position;
            newPos.x = telescopes[1].transform.position.x + telescopeOffsetX;
            telescopes[0].transform.position = newPos;
            SwapTelescopes();
        }
        else if (dragSpeed > 0 && telescopes[0].transform.position.x >= 0)
        {
            Vector3 newPos = telescopes[1].transform.position;
            newPos.x = telescopes[0].transform.position.x - telescopeOffsetX;
            telescopes[1].transform.position = newPos;
            SwapTelescopes();
        }

        if (dragSpeed == 0 && zoom)
        {
            for (int i = 0; i < telescopeElementsCount; i++)
            {
                if (telescopeElements[i].gameObject.activeInHierarchy && (Vector3.Distance(telescopeElements[i].transform.position, telescopeMask.position) <= detectionSensitivity))
                    telescopeElements[i].Trigger();
            }
        }
    }

    void SwapTelescopes()
    {
        GameObject temp = telescopes[0];
        telescopes[0] = telescopes[1];
        telescopes[1] = temp;
    }

    public void PlayAnimation(bool fadeIn, bool fadeOut, Sprite sprite = null)
    {
        if (sprite != null)
        {
            completeZone1.GetComponent<SpriteRenderer>().sprite = sprite;
            completeZone2.GetComponent<SpriteRenderer>().sprite = sprite;
        }

        if (fadeIn && fadeOut)
            fadeAnimator.Play("Base Layer.TelescopeFadeInOut");
        else if (fadeIn)
            fadeAnimator.Play("Base Layer.TelescopeFadeIn");
        else if (fadeOut)
            fadeAnimator.Play("Base Layer.TelescopeFadeOut");
            
    }
}
