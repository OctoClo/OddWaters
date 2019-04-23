using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roll : MonoBehaviour
{
    Vector3 initialPos = Vector3.zero;

    [Header("X values")]
    public bool rollX = true;
    [Range(0, 100)]
    public float xAmp = 10f;
    [Range(0, 100)]
    public float xSpeed = 8f;
    float xOffset;

    [Header("Y values")]
    public bool rollY = true;
    [Range(0, 100)]
    public float yAmp = 5f;
    [Range(0, 100)]
    public float ySpeed = 10f;
    float yOffset;

    [Header("Z values")]
    public bool rollZ = true;
    [Range(0, 100)]
    public float zAmp = 1f;
    [Range(0, 100)]
    public float zSpeed = 10f;
    float zOffset;

    [Header("Rotation")]
    public bool rotation = false;
    [Range(0, 100)]
    public float rotationAmp = 10f;
    [Range(0, 100)]
    public float rotationSpeed = 10f;
    float rotationOffset;

    float elapsedTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        initialPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {

        elapsedTime += Time.deltaTime;

        if (rollX)
            xOffset = Mathf.Sin(elapsedTime * (xSpeed / 100)) * (xAmp / 100);
        if (rollY)
            yOffset = Mathf.Cos(elapsedTime * (ySpeed / 100)) * (yAmp / 100);
        if (rollZ)
            zOffset = Mathf.Cos(elapsedTime * (zSpeed / 10)) * (zAmp / 100000);

        transform.position = new Vector3(initialPos.x + xOffset, initialPos.y + yOffset, transform.position.z + zOffset);

        if (rotation)
            rotationOffset = Mathf.Sin(elapsedTime * (rotationSpeed / 100)) * (rotationAmp / 100);

        transform.localEulerAngles = new Vector3(0f, 0f, rotationOffset);
    }
}
