using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBoundsZone : MonoBehaviour
{
    [SerializeField]
    InputManager inputManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("MouseProjection"))
        {
            inputManager.mouseProjectionOutOfDesk = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("MouseProjection"))
        {
            inputManager.mouseProjectionOutOfDesk = false;
        }
    }
}
