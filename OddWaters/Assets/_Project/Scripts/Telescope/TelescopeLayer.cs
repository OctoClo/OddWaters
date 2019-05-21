using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelescopeLayer : MonoBehaviour
{
    public float layerSize = 90;
    Vector3 initialPos;

    [SerializeField]
    bool parallax;
    public float parallaxSpeed = 1;

    [HideInInspector]
    public float dragSpeed;

    [HideInInspector]
    public Transform[] children;

    bool initialized = false;

    void Start()
    {
        Initialize();
    }

     void Initialize()
    {
        children = new Transform[2];
        children[0] = transform.GetChild(0);
        GameObject element2 = Instantiate(children[0].gameObject, transform);
        element2.name = "Element2";
        children[1] = element2.transform;

        initialPos = children[0].localPosition;
        Vector3 newPos = initialPos;
        newPos.x += parallaxSpeed * layerSize;
        children[1].localPosition = newPos;

        foreach (SpriteRenderer spriteRenderer in transform.GetComponentsInChildren<SpriteRenderer>())
            spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        initialized = true;
    }

    public void BeginDrag()
    {

    }

    public void EndDrag()
    {
        dragSpeed = 0;
    }

    public void ResetPosition()
    {
        if (!initialized)
            Initialize();

        children[0].localPosition = initialPos;
        Vector3 newPos = initialPos;
        newPos.x += parallaxSpeed * layerSize;
        children[1].localPosition = newPos;
    }

    void Update()
    {
        // Move both layers
        Vector3 move = new Vector3(dragSpeed, 0, 0);

        if (parallax)
            move *= parallaxSpeed;

        for (int i = 0; i < transform.childCount; i++)
        {
            children[0].localPosition += move;
            children[1].localPosition += move;
        }

        // Swap layers if needed
        if (dragSpeed < 0 && children[1].localPosition.x <= 0)
        {
            Vector3 newPos = children[0].localPosition;
            newPos.x = children[1].localPosition.x + parallaxSpeed * layerSize;
            children[0].localPosition = newPos;
            SwapLayers();
        }
        else if (dragSpeed > 0 && children[0].localPosition.x >= 0)
        {
            Vector3 newPos = children[1].localPosition;
            newPos.x = children[0].localPosition.x - parallaxSpeed * layerSize;
            children[1].localPosition = newPos;
            SwapLayers();
        }
    }

    void SwapLayers()
    {
        Transform temp = children[0];
        children[0] = children[1];
        children[1] = temp;
    }
}
