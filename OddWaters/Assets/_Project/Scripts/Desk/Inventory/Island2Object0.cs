using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island2Object0 : MonoBehaviour
{

    Color inactiveColor = Color.black;
    Color activeColor = new Color(57,191, 61);

    Color currentColor;

    Material mat;

    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        currentColor = Color.Lerp(inactiveColor, activeColor, Mathf.PingPong(Time.time, 1));

        mat.SetColor("_EmissionColor", currentColor / 20.0f);
    }
}
