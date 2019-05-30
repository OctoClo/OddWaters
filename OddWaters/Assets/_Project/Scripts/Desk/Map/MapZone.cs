using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapZone : MonoBehaviour
{
    [Header("General")]
    public int zoneNumber;
    public bool visible;

    [Header("Ambiance")]
    public AK.Wwise.State seaIntensity;
    public AK.Wwise.State weather;

    [Header("Panorama")]
    [SerializeField]
    List<GameObject> telescopePanoramas;
    int currentPanoramaIndex = -1;

    [Header("Activation")]
    [SerializeField]
    List<GameObject> elementsToHide = new List<GameObject>();

    [Header("References")]
    [SerializeField]
    GameObject clouds;
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        ListExtensions.Shuffle(telescopePanoramas);

        if (clouds != null && visible)
            //clouds.SetActive(!visible);
            animator.SetTrigger("RemoveClouds");

        foreach (GameObject element in elementsToHide)
            element.SetActive(false);
    }

    public void Discover()
    {
        visible = true;

        if (clouds != null)
            //clouds.SetActive(false);
            animator.SetTrigger("RemoveClouds");

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
