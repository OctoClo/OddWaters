using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanZone : MonoBehaviour
{
    [SerializeField]
    GameObject upPart;

    [SerializeField]
    bool goingRight;

    [SerializeField]
    float panSpeed = 0.5f;
    Vector3 panSpeedVec;

    bool pan;
    Transform mainCamera;

    void Start()
    {
        pan = false;
        mainCamera = transform.parent;
        panSpeedVec = new Vector3((goingRight ? panSpeed : -panSpeed), 0, 0);
    }

    void Update()
    {
        if (pan && ((goingRight && mainCamera.position.x < 8.4f) || (!goingRight && mainCamera.position.x > 0.2)))
        {
            mainCamera.position += panSpeedVec;
            upPart.transform.position += panSpeedVec;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("MouseProjection"))
        {
            pan = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("MouseProjection"))
        {
            pan = false;
        }
    }
}
