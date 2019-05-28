using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EScreenType { SEA, ISLAND_FULLSCREEN, ISLAND_SMALL }

public class ScreenManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Animator globalAnimator;
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
    [SerializeField]
    TutorialManager tutorialManager;
    bool tutorial;
    [SerializeField]
    GameObject tutorialPanel;

    [HideInInspector]
    public EScreenType screenType = EScreenType.SEA;

    [HideInInspector]
    public int currentIslandNumber;
    Island currentIsland;
    bool firstVisit;
    GameObject objectToGive;
    int nextZone;

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
        globalAnimator.ResetTrigger("EndNavigationAtSea");
        globalAnimator.SetTrigger("BeginNavigation");
    }

    public void EndNavigationAtSea()
    {
        globalAnimator.ResetTrigger("LeaveIsland");
        globalAnimator.SetTrigger("EndNavigationAtSea");
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

        EventManager.Instance.Raise(new BlockInputEvent() { block = true, navigation = false });

        globalAnimator.SetTrigger("FirstBerth");
        if (tutorialNow) tutorialManager.CompleteStep();
        yield return new WaitForSeconds(4);
        dialogueManager.StartDialogue(island.dialogue, firstVisit);
    }

    public IEnumerator RelaunchDialogue()
    {
        EventManager.Instance.Raise(new BlockInputEvent() { block = true, navigation = false });
        globalAnimator.SetTrigger("Retalk");
        yield return new WaitForSeconds(1.9f);
        dialogueManager.StartDialogue(currentIsland.dialogue, false);
    }

    public IEnumerator TransitionAfterFirstBerth(bool firstEncounter)
    {
        globalAnimator.SetTrigger("EndDialogue");
        yield return new WaitForSeconds(1.5f);
        if (firstEncounter)
        {
            // Add object to inventory
            objectToGive = currentIsland.objectToGive;
            bool waitLonger = inventory.TradeObjects(objectToGive);
            AkSoundEngine.PostEvent("Play_Island" + currentIslandNumber + "_Object0", gameObject);
            yield return new WaitForSeconds(2.5f + (waitLonger ? 1 : 0));

            // Discover new zone
            nextZone = currentIsland.nextZone;
            EventManager.Instance.Raise(new DiscoverZoneEvent() { zoneNumber = nextZone });

            // Tutorial
            if (tutorial)
            {
                //tutorial = false;
                //tutorialPanel.SetActive(true);
                tutorialManager.NextStep();
            }
        }
        else
            EventManager.Instance.Raise(new BlockInputEvent() { block = false, navigation = false });
    }

    public void LeaveIsland()
    {
        globalAnimator.ResetTrigger("Berth");
        globalAnimator.ResetTrigger("FirstBerth");
        AkSoundEngine.PostEvent("Stop_AMB_Island" + currentIslandNumber, gameObject);
        globalAnimator.SetTrigger("LeaveIsland");
        currentIslandNumber = -1;
    }

    void OnDialogueEvent(DialogueEvent e)
    {
        if (!e.ongoing)
            StartCoroutine(TransitionAfterFirstBerth(e.firstEncounter));
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
