using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelescopeLayer : MonoBehaviour
{
    public float layerSize = 90;

    [SerializeField]
    bool parallax;
    public float parallaxSpeed = 1;
    Transform randomChild;
    float lastX;

    [HideInInspector]
    public float dragSpeed;

    [HideInInspector]
    public Transform[] children;

    void Start()
    {
        children = new Transform[2];
        children[0] = transform.GetChild(0);
        GameObject element2 = Instantiate(children[0].gameObject, transform);
        element2.name = "Element2";
        children[1] = element2.transform;

        children[0].localPosition = new Vector3(0, 0, 0);
        children[1].localPosition = new Vector3(parallaxSpeed * layerSize, 0, 0);

        randomChild = children[0];
        lastX = randomChild.localPosition.x;
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
        children[0].localPosition = Vector3.zero;
        Vector3 newPosition = children[0].localPosition;
        newPosition.x += parallaxSpeed * layerSize;
        children[1].localPosition = newPosition;
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
            lastX = randomChild.localPosition.x;
            SwapLayers();
        }
        else if (dragSpeed > 0 && children[0].localPosition.x >= 0)
        {
            Vector3 newPos = children[1].localPosition;
            newPos.x = children[0].localPosition.x - parallaxSpeed * layerSize;
            children[1].localPosition = newPos;
            lastX = randomChild.localPosition.x;
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
