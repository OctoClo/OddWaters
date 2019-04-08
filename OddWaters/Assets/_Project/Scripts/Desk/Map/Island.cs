using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island : MonoBehaviour
{
    public int islandNumber;
    public Sprite illustration;
    public Sprite character;
    public GameObject objectToGive;
    [HideInInspector]
    public bool firstTimeVisiting = true;

    public void Berth()
    {
        firstTimeVisiting = false;
    }
}
