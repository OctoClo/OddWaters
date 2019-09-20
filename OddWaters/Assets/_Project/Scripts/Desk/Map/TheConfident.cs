using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheConfident : MapElement
{
    [Header("The Confident")]
    [SerializeField]
    Inventory inventory;
    [SerializeField]
    GameObject lastNote; 

    override public IEnumerator Discover(bool tutorial, TutorialManager tutorialManager)
    {
        yield return StartCoroutine(base.Discover(tutorial, tutorialManager));
        inventory.AddObjectToInventory(false, lastNote);
    }
}
