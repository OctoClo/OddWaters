﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Telescope : MonoBehaviour
{
    [SerializeField]
    Sprite cursorCenter;
    GameObject cursorBegin;
    Vector3 cursorScale;
    
    [SerializeField]
    GameObject telescope1;
    GameObject telescope2;
    Transform telescopeParent;
    Transform telescopeMask;
    GameObject[] telescopes;
    GameObject completeZone1;
    GameObject completeZone2;
    GameObject elementsFolder1;
    GameObject elementsFolder2;
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

    [SerializeField]
    Animator fadeAnimator;

    [SerializeField]
    Boat boat;

    void Start()
    {
        cursorScale = new Vector3(1f, 1f, 0);

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

        elementsFolder1 = telescope1.transform.GetChild(1).gameObject;
        elementsFolder2 = telescope2.transform.GetChild(1).gameObject;
        GameObject element1, element2;

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
            /*cursorBegin = new GameObject("CursorBegin");
            beginPos.y = 0;
            cursorBegin.transform.position = beginPos;
            cursorBegin.transform.rotation = Quaternion.Euler(90, 0, 0);
            cursorBegin.transform.localScale = cursorScale;
            SpriteRenderer renderer = cursorBegin.AddComponent<SpriteRenderer>();
            renderer.sprite = cursorCenter;
            renderer.sortingOrder = 2;*/
            dragInitialized = true;
        }
    }

    public void EndDrag()
    {
        dragSpeed = 0;
        Destroy(cursorBegin);
        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
        dragInitialized = false;
    }

    public void UpdateSpeed(float speed)
    {
        dragSpeed = speed * (zoom ? dragSpeedMultiplierZoom : dragSpeedMultiplier);
        if (dragSpeed == 0)
            CursorManager.Instance.SetCursor(ECursor.TELESCOPE_PAN_CENTER);
        else if (dragSpeed < 0)
            CursorManager.Instance.SetCursor(ECursor.TELESCOPE_PAN_RIGHT);
        else
            CursorManager.Instance.SetCursor(ECursor.TELESCOPE_PAN_LEFT);
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
            int islandCount = elementsFolder1.transform.childCount;
            for (int i = 0; i < islandCount; i++)
            {
                TelescopeElement island3D1 = elementsFolder1.transform.GetChild(i).GetComponent<TelescopeElement>();
                if (!island3D1.islandDiscover.visible)
                {
                    if (island3D1.gameObject.activeInHierarchy && (Vector3.Distance(island3D1.transform.position, telescopeMask.position) <= detectionSensitivity))
                        island3D1.Trigger();

                    TelescopeElement island3D2 = elementsFolder2.transform.GetChild(i).GetComponent<TelescopeElement>();
                    if (island3D2.gameObject.activeInHierarchy && (Vector3.Distance(island3D2.transform.position, telescopeMask.position) <= detectionSensitivity))
                        island3D2.Trigger();
                }
            }
        }
    }

    void SwapTelescopes()
    {
        GameObject temp = telescopes[0];
        telescopes[0] = telescopes[1];
        telescopes[1] = temp;
    }

    public IEnumerator PlayAnimation(bool fadeIn, bool fadeOut, Sprite sprite = null)
    {
        if (sprite != null)
        {
            completeZone1.GetComponent<SpriteRenderer>().sprite = sprite;
            completeZone2.GetComponent<SpriteRenderer>().sprite = sprite;
        }

        if (fadeIn && fadeOut)
            fadeAnimator.Play("Base Layer.TelescopeFadeInOut");
        else
        {
            if (fadeIn)
            {
                int islandCount = elementsFolder1.transform.childCount;
                for (int i = 0; i < islandCount; i++)
                {
                    Destroy(elementsFolder1.transform.GetChild(i).gameObject);
                    Destroy(elementsFolder2.transform.GetChild(i).gameObject);
                }
                fadeAnimator.Play("Base Layer.TelescopeFadeIn");
            }
            else if (fadeOut)
            {
                fadeAnimator.Play("Base Layer.TelescopeFadeOut");
                yield return new WaitForSeconds(0.5f);
                foreach (Island island in boat.GetIslandsInSight())
                {
                    GameObject island3D1 = Instantiate(island.island3D, elementsFolder1.transform);
                    GameObject island3D2 = Instantiate(island.island3D, elementsFolder2.transform);
                    TelescopeElement island3D1Element = island3D1.GetComponent<TelescopeElement>();
                    TelescopeElement island3D2Element = island3D2.GetComponent<TelescopeElement>();
                    island3D1Element.cloneElement = island3D2;
                    island3D2Element.cloneElement = island3D1;
                    island3D1Element.islandDiscover = island;
                    island3D2Element.islandDiscover = island;
                }
            }
        }
    }

    public void ResetAnimation()
    {
        fadeAnimator.Play("Base Layer.Default");
    }
}
