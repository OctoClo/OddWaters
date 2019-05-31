using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneStep : MapElement
{
    [Header("Safe Zone")]
    [SerializeField]
    AK.Wwise.State state;

    protected override void Start()
    {
    }

    public override IEnumerator Discover(bool tutorial, TutorialManager tutorialManager)
    {
        visible = true;
        discovered = true;
        state.SetValue();

        yield return null;
    }
}
