using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island2Object0 : Interactible
{
    [Header("Activation")]
    [SerializeField]
    Color activeColor = new Color(2, 58, 6);
    Material mat;
    [SerializeField]
    float maxTotalAngle = 45;
    [SerializeField]
    float maxHeight = 2;

    TelescopeElement telescopeElement;

    bool activated;
    bool tracking;
    public bool floating;
    Vector3 beginFloatPos;
    Vector3 targetFloatPos;
    Roll roll;

    float angleFactor;
    public float currentAngle;
    public float currentPercentage;

    protected override void Start()
    {
        base.Start();

        mat = GetComponent<Renderer>().material;
        roll = GetComponent<Roll>();

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

    protected override void Update()
    {
        base.Update();

        if (tracking)
        {
            currentAngle = telescopeElement.angleToBoat;
            if (currentAngle > 180)
                currentAngle = 360 - currentAngle;
            if (currentAngle <= maxTotalAngle)
            {
                currentPercentage = 1 - currentAngle * angleFactor;
                if (!floating)
                {
                    rigidBody.useGravity = false;
                    beginFloatPos = transform.position;
                }
                floating = true;
            }
            else
            {
                currentPercentage = 0;
                if (floating)
                    rigidBody.useGravity = true;
                floating = false;
            }

            if (floating && !zoom && !grabbed)
            {
                targetFloatPos = beginFloatPos;
                targetFloatPos.y += maxHeight * currentPercentage;
                rigidBody.velocity = (targetFloatPos - transform.position) * 1;
            }

            if (telescopeElement.elementDiscover.discovered)
            {
                activated = true;
                tracking = false;
                mat.SetColor("_EmissionColor", activeColor / 10.0f);
            }
        }
    }

    public override bool IsGrabbable()
    {
        bool grabbable;
        float velocityMag = rigidBody.velocity.sqrMagnitude;

        if (floating)
            grabbable = velocityMag <= 0.9f;
        else
            grabbable = velocityMag <= 0.5f;

        return grabbable;
    }

    public override void Grab()
    {
        if (tracking || activated)
            roll.enabled = false;
        AkSoundEngine.PostEvent("Play_Manipulation", gameObject);
        rigidBody.useGravity = false;
        transform.position = GetGrabbedPosition();
        grabbed = true;
    }

    public override void Drop()
    {
        AkSoundEngine.PostEvent("Play_Manipulation", gameObject);
       
        if (tracking || activated)
        {
            roll.enabled = true;
            beginFloatPos.x = transform.position.x;
            beginFloatPos.z = transform.position.z;
        }

        grabbed = false;

        if (!floating)
            rigidBody.useGravity = true;
    }

    public override Vector3 GetGrabbedPosition()
    {
        if (!floating)
            return base.GetGrabbedPosition();
        else
            return gameObject.transform.position;
    }

    public override void EnterRotationInterface()
    {
        if (tracking || activated)
        {
            roll.enabled = false;
            currentZoomOffset = zoomOffset - (beginFloatPos.y + maxHeight * currentPercentage);
        }
        else
            currentZoomOffset = zoomOffset;
        base.EnterRotationInterface();
    }

    public override void ExitRotationInterface()
    {
        if (rotating)
        {
            rotating = false;
            currentRotationSpeed = rotationSpeed;
            transform.rotation = rotationAfter;
        }

        AkSoundEngine.PostEvent("Play_Manipulation", gameObject);
        zoom = false;
        transform.parent = inventory;
        foreach (Collider collider in colliders)
            collider.isTrigger = false;

        if (tracking || activated)
            StartCoroutine(WaitBeforeEnablingRoll(0.5f));

        if (!floating)
        {
            rigidBody.useGravity = true;
            beforeZoomPosition.y += 0.5f;
        }

        gameObject.transform.position = beforeZoomPosition;
    }

    IEnumerator WaitBeforeEnablingRoll(float delay)
    {
        yield return new WaitForSeconds(delay);
        roll.enabled = true;
    }

    void OnMegaTyphoonActivatedEvent(MegaTyphoonActivatedEvent e)
    {
        if (!activated)
        {
            tracking = true;
            telescopeElement = e.element;
            roll.enabled = true;
        }
    }
}
