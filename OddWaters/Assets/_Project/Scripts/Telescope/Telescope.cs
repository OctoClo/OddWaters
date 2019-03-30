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
    GameObject telescopeParent;
    GameObject[] telescopes;
    GameObject completeZone1;
    GameObject completeZone2;
    Sprite sprite1;
    Sprite sprite2;
    float telescopeOffsetX;

    bool dragInitialized;
    const float dragSpeedMultiplier = 0.01f;
    const float dragSpeedMultiplierZoom = 0.001f;
    float dragSpeed;

    [SerializeField]
    float zoomLevel = 1.5f;
    Vector3 scaleZoom;
    Vector3 scaleNormal;
    bool zoom;

    [SerializeField]
    Animator fadeAnimator;

    void Start()
    {
        cursorOffset = new Vector2(cursorCenter.texture.width / 2, cursorCenter.texture.height / 2);
        cursorScale = new Vector3(1.5f, 1.5f, 0);

        completeZone1 = telescope1.transform.GetChild(0).gameObject;
        sprite1 = completeZone1.GetComponent<SpriteRenderer>().sprite;
        telescopeOffsetX = sprite1.texture.width * completeZone1.transform.localScale.x / sprite1.pixelsPerUnit;
        telescopeParent = telescope1.transform.parent.gameObject;

        telescope2 = Instantiate(telescope1, transform);
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

        int telescopeElementsCount = elementsFolder1.transform.childCount;
        for (int i = 0; i < telescopeElementsCount; i++)
        {
            element1 = elementsFolder1.transform.GetChild(i).gameObject;
            element2 = elementsFolder2.transform.GetChild(i).gameObject;
            element1.GetComponent<TelescopeElement>().cloneElement = element2;
            element2.GetComponent<TelescopeElement>().cloneElement = element1;
        }

        dragSpeed = 0;
        dragInitialized = false;

        zoom = false;
        scaleZoom = new Vector3(zoomLevel, 1, zoomLevel);
        scaleNormal = new Vector3(1, 1, 1);
    }

    public void Zoom(bool zoomed)
    {
        zoom = zoomed;
        Vector3 scale = (zoom ? scaleZoom : scaleNormal);
        telescopeParent.transform.localScale = scale;
        telescopeParent.transform.localScale = scale;
        telescopeOffsetX = sprite1.texture.width * completeZone1.transform.localScale.x * scale.x / sprite1.pixelsPerUnit;
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
            renderer.sortingOrder = 1;
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
            Cursor.SetCursor(cursorLeft.texture, cursorOffset, CursorMode.Auto);
        else
            Cursor.SetCursor(cursorRight.texture, cursorOffset, CursorMode.Auto);
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
