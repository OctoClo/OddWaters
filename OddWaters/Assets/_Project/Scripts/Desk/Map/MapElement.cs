using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapElement : MonoBehaviour
{
    public new string name;
    public bool needSight = true;
    public bool needZoom = false;
    public bool needSuperPrecision = false;
    public bool visible;
    public bool magnetism = true;
    public bool playClue = true;
    public Sprite elementSprite;
    public ELayer layer;

    [SerializeField]
    AK.Wwise.Event discoverySound;

    [HideInInspector]
    public bool discovered = false;

    MeshRenderer meshRenderer;

    [SerializeField]
    GameObject[] elementsToActivate;

    virtual protected void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = visible;
    }

    virtual public IEnumerator Discover(bool tutorial, TutorialManager tutorialManager)
    {
        if (discoverySound != null)
        {
            discoverySound.Post(gameObject);
            yield return new WaitForSeconds(1.5f);
        }

        AkSoundEngine.PostEvent("Play_Note", gameObject);
        meshRenderer.enabled = true;
        visible = true;
        discovered = true;

        if (tutorial)
            tutorialManager.CompleteStep();

        for (int i = 0; i < elementsToActivate.Length; i++)
            elementsToActivate[i].SetActive(true);
    }
}
