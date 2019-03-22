using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    [SerializeField]
    Map map;

    [SerializeField]
    Telescope telescope;

    int currentZone = -1;

    void Update()
    {
        if (map.currentZone != currentZone)
        {
            currentZone = map.currentZone;
            StartCoroutine(telescope.ChangeTexture(map.GetNewZoneTexture()));
        }
    }
}
