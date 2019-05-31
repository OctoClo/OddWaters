using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island2Object0 : MonoBehaviour
{
    [Header("Glow")]
    [SerializeField]
    Color inactiveColor = Color.black;
    [SerializeField]
    Color activeColor = new Color(2, 58, 6);
    [SerializeField]
    float maxTotalAngle = 45;

    Color currentColor;
    Material mat;

    bool activated;
    TelescopeElement telescopeElement;
    bool tracking;
    public float angleFactor;
    public float currentAngle;
    public float currentPercentage;

    void Start()
    {
        mat = GetComponent<Renderer>().material;

        maxTotalAngle /= 2.0f;
        activated = false;
        tracking = false;
        angleFactor = 1.0f / maxTotalAngle;
    }

    void OnEnable()
    {
        EventManager.Instance.AddListener<MegaTyphoonActivatedEvent>(OnMegaTyphoonActivatedEvent);
    }

    void OnDisable()
    {
        EventManager.Instance.RemoveListener<MegaTyphoonActivatedEvent>(OnMegaTyphoonActivatedEvent);
    }

    void Update()
    {
        if (tracking)
        {
            currentAngle = telescopeElement.angleToBoat;
            if (currentAngle > 180)
                currentAngle = 360 - currentAngle;
            if (currentAngle <= maxTotalAngle)
                currentPercentage = 1 - currentAngle * angleFactor;
            else
                currentPercentage = 0;

            currentColor = Color.Lerp(inactiveColor, activeColor, currentPercentage);
            mat.SetColor("_EmissionColor", currentColor / 20.0f);

            if (telescopeElement.elementDiscover.discovered)
            {
                activated = true;
                tracking = false;
                mat.SetColor("_EmissionColor", activeColor / 10.0f);
            }
        }
    }

    void OnMegaTyphoonActivatedEvent(MegaTyphoonActivatedEvent e)
    {
        if (!activated)
        {
            tracking = true;
            telescopeElement = e.element;
        }
    }
}
