using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island1Object0 : MonoBehaviour
{
    [SerializeField]
    float maxDistanceToStone;
    [SerializeField]
    MapElement currentStone;
    [SerializeField]
    float currentPercentage;

    float distanceFactor;
    float distanceToCurrentStone;
    Vector3 belowPos;

    [SerializeField]
    Renderer[] emissionRenderers;
    [SerializeField]
    Color inactiveColor;
    [SerializeField]
    Color activeColor;

    Color currentColor;
    Material[] materials;
    int renderersCount;

    void Start()
    {
        distanceFactor = 1 / maxDistanceToStone;

        renderersCount = emissionRenderers.Length;
        materials = new Material[renderersCount];
        for (int i = 0; i < renderersCount; i++)
            materials[i] = emissionRenderers[i].material;
    }

    void OnEnable()
    {
        EventManager.Instance.AddListener<BoatInMapElement>(OnBoatInMapElement);
    }

    void OnDisable()
    {
        EventManager.Instance.RemoveListener<BoatInMapElement>(OnBoatInMapElement);
    }

    void Update()
    {
        if (currentStone)
        {
            belowPos = transform.position;
            belowPos.y = currentStone.transform.position.y;
            distanceToCurrentStone = Mathf.Clamp((currentStone.transform.position - belowPos).sqrMagnitude, 0, maxDistanceToStone);
            currentPercentage = 1 - distanceToCurrentStone * distanceFactor;

            AkSoundEngine.SetRTPCValue("Pulse", currentPercentage);

            currentColor = Color.Lerp(inactiveColor, activeColor, currentPercentage);
            for (int i = 0; i < renderersCount; i++)
                materials[i].SetColor("_EmissionColor", currentColor);
        }
    }

    void OnBoatInMapElement(BoatInMapElement e)
    {
        if (!e.exit)
            currentStone = e.elementZone.GetComponentInParent<MapElement>();
        else
            currentStone = null;
    }
}
