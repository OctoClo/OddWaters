using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EScreenType { SEA, ISLAND_FULLSCREEN, ISLAND_SMALL }

public class ScreenManager : MonoBehaviour
{
    [SerializeField]
    Animator upPartAnimator;

    [SerializeField]
    GameObject telescopeScreen;

    [SerializeField]
    GameObject desk;

    [SerializeField]
    Inventory inventory;
    [SerializeField]
    DialogueManager dialogueManager;

    [SerializeField]
    GameObject islandScreen;
    [SerializeField]
    GameObject islandBackground;
    [SerializeField]
    GameObject islandCharacter;

    [HideInInspector]
    public EScreenType screenType = EScreenType.SEA;

    [HideInInspector]
    public int currentIslandNumber;
    Island currentIsland;
    bool firstVisit;
    GameObject objectToGive;
    int nextZone;

    [SerializeField]
    TutorialManager tutorialManager;
    bool tutorial;
    [SerializeField]
    GameObject tutorialPanel;

    private void Start()
    {
        currentIslandNumber = -1;
        tutorial = false;
    }

    void OnEnable()
    {
        EventManager.Instance.AddListener<DialogueEvent>(OnDialogueEvent);
    }

    void OnDisable()
    {
        EventManager.Instance.RemoveListener<DialogueEvent>(OnDialogueEvent);
    }

    public void BeginNavigation()
    {
        upPartAnimator.ResetTrigger("EndNavigationAtSea");
        upPartAnimator.SetTrigger("BeginNavigation");
    }

    public void EndNavigationAtSea()
    {
        upPartAnimator.ResetTrigger("LeaveIsland");
        upPartAnimator.SetTrigger("EndNavigationAtSea");
    }

    public IEnumerator Berth(Island island, bool tutorialNow)
    {
        currentIsland = island;
        tutorial = tutorialNow;

        if (!island.discovered)
        {
            yield return StartCoroutine(island.Discover(tutorialNow, tutorialManager));
            yield return new WaitForSeconds(1);
        }

        islandBackground.GetComponent<SpriteRenderer>().sprite = island.background;
        islandCharacter.GetComponent<SpriteRenderer>().sprite = island.character;

        currentIslandNumber = island.islandNumber;
        firstVisit = island.firstTimeVisiting;
        island.Berth();

        if (firstVisit)
        {
            upPartAnimator.SetTrigger("FirstBerth");
            tutorialPanel.SetActive(false);
            yield return new WaitForSeconds(5);
            dialogueManager.StartDialogue(island.dialogue);
        }
        else
        {
            upPartAnimator.SetTrigger("Berth");
            EventManager.Instance.Raise(new BlockInputEvent() { block = false });
        }
    }

    public IEnumerator TransitionAfterFirstBerth()
    {
        upPartAnimator.SetTrigger("EndDialogue");
        yield return new WaitForSeconds(2.5f);

        // Add object to inventory
        objectToGive = currentIsland.objectToGive;
        inventory.AddToInventory(objectToGive);
        AkSoundEngine.PostEvent("Play_Island" + currentIslandNumber + "_Object0", gameObject);

        // Discover new zone
        yield return new WaitForSeconds(2.5f);
        nextZone = currentIsland.nextZone;
        EventManager.Instance.Raise(new DiscoverZoneEvent() { zoneNumber = nextZone });

        // Tutorial
        if (tutorial)
        {
            tutorial = false;
            tutorialPanel.SetActive(true);
            tutorialManager.NextStep();
        }

        EventManager.Instance.Raise(new BlockInputEvent() { block = false });
    }

    public void LeaveIsland()
    {
        upPartAnimator.ResetTrigger("Berth");
        upPartAnimator.ResetTrigger("FirstBerth");
        AkSoundEngine.PostEvent("Stop_AMB_Island" + currentIslandNumber, gameObject);
        upPartAnimator.SetTrigger("LeaveIsland");
        currentIslandNumber = -1;
    }

    void OnDialogueEvent(DialogueEvent e)
    {
        if (!e.ongoing)
            StartCoroutine(TransitionAfterFirstBerth());
    }

    void ChangeScreenType(EScreenType newType)
    {
        screenType = newType;

        if (screenType == EScreenType.ISLAND_FULLSCREEN)
        {
            desk.SetActive(false);
            telescopeScreen.SetActive(false);
            islandScreen.SetActive(true);
        }
        else if (screenType == EScreenType.ISLAND_SMALL)
        {
            desk.SetActive(true);
            telescopeScreen.SetActive(false);
            islandScreen.SetActive(true);
        }
        else
        {
            islandScreen.SetActive(false);
            telescopeScreen.SetActive(true);
        }
    }
}
