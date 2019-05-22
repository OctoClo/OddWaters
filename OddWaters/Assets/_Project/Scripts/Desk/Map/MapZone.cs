﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapZone : MonoBehaviour
{
    public int zoneNumber;
    [SerializeField]
    List<GameObject> telescopePanoramas;
    int currentPanoramaIndex = -1;
    
    public bool visible;

    [SerializeField]
    Material visibleMat;
    [SerializeField]
    Material invisibleMat;

    MeshRenderer meshRenderer;

    [SerializeField]
    List<GameObject> elementsToHide = new List<GameObject>();

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = (visible ? visibleMat : invisibleMat);
        ListExtensions.Shuffle(telescopePanoramas);

        foreach (GameObject element in elementsToHide)
            element.SetActive(false);
    }

    public void Discover()
    {
        visible = true;
        meshRenderer.material = visibleMat;
        foreach (GameObject element in elementsToHide)
            element.SetActive(true);
    }

    public GameObject GetPanorama()
    {
        currentPanoramaIndex++;
        if (currentPanoramaIndex >= telescopePanoramas.Count)
        {
            currentPanoramaIndex = 0;
            ListExtensions.Shuffle(telescopePanoramas);
        }

        return telescopePanoramas[currentPanoramaIndex];
    }
}
