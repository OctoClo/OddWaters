using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanZone : MonoBehaviour
{
    [SerializeField]
    GameObject telescope;

    [SerializeField]
    GameObject islandScreen;

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
        if (pan && ((goingRight && mainCamera.position.x < 9.6) || (!goingRight && mainCamera.position.x > 0.2)))
        {
            mainCamera.position += panSpeedVec;
            telescope.transform.position += panSpeedVec;
            islandScreen.transform.position += panSpeedVec;
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
