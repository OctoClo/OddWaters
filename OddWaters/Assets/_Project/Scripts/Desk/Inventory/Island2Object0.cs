using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island2Object0 : MonoBehaviour
{

    Color inactiveColor = Color.black;
    Color activeColor = new Color(2, 58, 6);

    Color currentColor;

    Material mat;

    public bool active = false;

    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        

        if(active)
        {
            currentColor = Color.Lerp(inactiveColor, activeColor, Mathf.PingPong(Time.time, 1));
        }
        else
        {
            currentColor = inactiveColor;
        }

        mat.SetColor("_EmissionColor", currentColor / 20.0f);
    }
}
