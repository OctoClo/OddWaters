using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapZone : MonoBehaviour
{
    public int zoneNumber;
    [SerializeField]
    GameObject[] telescopePanoramas;
    int currentPanoramaIndex = -1;
    
    public bool visible;

    [SerializeField]
    Material visibleMat;
    [SerializeField]
    Material invisibleMat;

    MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = (visible ? visibleMat : invisibleMat);
    }

    public void Discover()
    {
        visible = true;
        meshRenderer.material = visibleMat;
    }

    public GameObject GetPanorama()
    {
        currentPanoramaIndex = (currentPanoramaIndex + 1) % telescopePanoramas.Length;
        return telescopePanoramas[currentPanoramaIndex];
    }
}
