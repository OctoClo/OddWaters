using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island2Object0 : Interactible
{
    [Header("Clues")]
    [SerializeField]
    Texture emissiveClue;
    Color inactiveColor = Color.black;
    public Color currentColor;
    [SerializeField]
    float maxTotalAngle = 45;
    [SerializeField]
    float maxHeight = 2;

    [Header("Activation")]
    [SerializeField]
    Texture emissiveActive;
    [SerializeField]
    Color activeColor = new Color(2, 58, 6);
    [SerializeField]
    TextAsset clueJSON;

    Material mat;
    TelescopeElement telescopeElement;

    bool activated;
    bool tracking;
    bool onIsland;
    Vector3 beforeDialoguePos;
    Vector3 beginFloatPos;
    Vector3 targetFloatPos;
    Transcript clueTranscript;

    [Header("Debug")]
    public bool floating;
    float angleFactor;
    public float currentAngle;
    public float currentPercentage;

    protected override void Start()
    {
        base.Start();

        mat = GetComponent<Renderer>().material;
        mat.SetTexture("_EmissionMap", emissiveClue);

        maxTotalAngle /= 2.0f;
        activated = false;
        tracking = false;
        angleFactor = 1.0f / maxTotalAngle;
        onIsland = false;

        clueTranscript = JsonUtility.FromJson<Transcript>(clueJSON.text);
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
            // Calculate current %
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
                    transcriptVerso = clueTranscript;
                }
                floating = true;
            }
            else
            {
                currentPercentage = 0;
                if (floating)
                {
                    rigidBody.useGravity = true;
                    transcriptVerso = null;
                }
                    
                floating = false;
            }

            // Floating
            if (floating && !zoom && !grabbed && !onIsland)
            {
                targetFloatPos = beginFloatPos;
                targetFloatPos.y += maxHeight * currentPercentage;
                rigidBody.velocity = (targetFloatPos - transform.position) * 1;
            }

            // Glowing
            currentColor = Color.Lerp(inactiveColor, activeColor, currentPercentage);
            mat.SetColor("_EmissionColor", currentColor / 10.0f);

            // Activation
            if (telescopeElement.elementDiscover.discovered)
            {
                activated = true;
                tracking = false;
                mat.SetTexture("_EmissionMap", emissiveActive);
                mat.SetColor("_EmissionColor", activeColor / 10.0f);
                rigidBody.useGravity = true;
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

        if (!floating)
        {
            rigidBody.useGravity = true;
            beforeZoomPosition.y += 0.5f;
        }

        gameObject.transform.position = beforeZoomPosition;
    }

    public void HandleBerth(bool berth)
    {
        if (tracking || activated)
        {
            onIsland = berth;

            if (onIsland)
            {
                beforeDialoguePos = transform.position;
                rigidBody.useGravity = true;
            }
            else if (!onIsland)
            {
                rigidBody.useGravity = false;
                transform.position = beforeDialoguePos;
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
