using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTest : Interactible
{
    public override void Trigger()
    {
        base.Trigger();
        Debug.Log("Mouse on ObjectTest !");
    }
}
