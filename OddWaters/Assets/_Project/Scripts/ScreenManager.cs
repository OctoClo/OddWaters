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
    GameObject islandScreen;
    [SerializeField]
    GameObject islandBackground;
    [SerializeField]
    GameObject islandCharacter;

    [HideInInspector]
    public EScreenType screenType = EScreenType.SEA;

    [HideInInspector]
    public int currentIslandNumber;
    bool firstVisit;
    GameObject objectToGive;
    int nextZone;

    private void Start()
    {
        AkSoundEngine.SetState("SeaIntensity", "CalmSea");
        AkSoundEngine.SetState("Weather", "Fine");
        AkSoundEngine.PostEvent("Play_AMB_Sea", gameObject);
        currentIslandNumber = -1;
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
        Debug.Log("EndNavigationAtSea");
    }

    public IEnumerator Berth(Island island)
    {
        if (!island.discovered)
        {
            yield return StartCoroutine(island.Discover());
            yield return new WaitForSeconds(1);
        }

        islandBackground.GetComponent<SpriteRenderer>().sprite = island.background;
        islandCharacter.GetComponent<SpriteRenderer>().sprite = island.character;

        currentIslandNumber = island.islandNumber;
        firstVisit = island.firstTimeVisiting;
        island.Berth();

        if (firstVisit)
        {
            upPartAnimator.SetTrigger("Discover");
            
            // Add object to inventory
            yield return new WaitForSeconds(7);
            objectToGive = island.objectToGive;
            inventory.AddToInventory(objectToGive);
            AkSoundEngine.PostEvent("Play_Island" + currentIslandNumber + "_Object0", gameObject);

            // Discover new zone
            yield return new WaitForSeconds(2.5f);
            nextZone = island.nextZone;
            EventManager.Instance.Raise(new DiscoverZoneEvent() { zoneNumber = nextZone });
        }
        else
            upPartAnimator.SetTrigger("Berth");

        EventManager.Instance.Raise(new BlockInputEvent() { block = false });
    }

    public void LeaveIsland()
    {
        upPartAnimator.ResetTrigger("Berth");
        AkSoundEngine.PostEvent("Stop_AMB_Island" + currentIslandNumber, gameObject);
        upPartAnimator.SetTrigger("LeaveIsland");
        currentIslandNumber = -1;
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
