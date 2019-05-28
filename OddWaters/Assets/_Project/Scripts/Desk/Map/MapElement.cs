using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapElement : MonoBehaviour
{
    [Header("General")]
    public new string name;
    public bool visible;

    [Header("Discovery")]
    public bool needSight = true;
    public bool needZoom = false;
    public bool needSuperPrecision = false;
    public bool magnetism = true;
    [SerializeField]
    GameObject[] elementsToActivate;

    [Header("Sound")]
    public bool playClue = true;
    [SerializeField]
    AK.Wwise.Event discoverySound;

    [Header("Panorama")]
    public Sprite elementSprite;
    public ELayer layer;

    [HideInInspector]
    public bool discovered = false;
    MeshRenderer meshRenderer;

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
