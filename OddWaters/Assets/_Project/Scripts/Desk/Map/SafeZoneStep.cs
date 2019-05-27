using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneStep : MapElement
{
    protected override void Start()
    {
    }

    public override IEnumerator Discover(bool tutorial, TutorialManager tutorialManager)
    {
        visible = true;
        discovered = true;
        AkSoundEngine.SetState("Seavoices_State", name);

        yield return null;
    }
}
