using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ERainType
{
    NONE,
    LIGHT,
    HEAVY
}

public class RainManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] heavyRainVFX;
    [SerializeField]
    GameObject[] lightRainVFX;

    public void UpdateRain(ERainType rainType)
    {
        foreach (GameObject rainVFX in heavyRainVFX)
            rainVFX.SetActive(rainType == ERainType.HEAVY);
            

        foreach (GameObject rainVFX in lightRainVFX)
            rainVFX.SetActive(rainType == ERainType.LIGHT);
    }
}
