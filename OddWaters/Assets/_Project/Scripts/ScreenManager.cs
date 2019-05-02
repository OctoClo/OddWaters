using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EScreenType { SEA, ISLAND_FULLSCREEN, ISLAND_SMALL }

public enum EIslandIlluType { FULLSCREEN, SMALL, COUNT }

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
    GameObject[] islandFolders;
    [SerializeField]
    GameObject[] islandIllustrations;
    [SerializeField]
    GameObject[] islandCharacters;

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

        for (int i = 0; i < (int)EIslandIlluType.COUNT; i++)
        {
            islandIllustrations[i].GetComponent<SpriteRenderer>().sprite = island.illustration;
            islandCharacters[i].GetComponent<SpriteRenderer>().sprite = island.character;
        }

        currentIslandNumber = island.islandNumber;
        firstVisit = island.firstTimeVisiting;
        island.Berth();

        if (firstVisit)
        {
            objectToGive = island.objectToGive;
            nextZone = island.nextZone;

            //Play arrival animation
            upPartAnimator.SetTrigger("Discover");

            //islandScreen.SetActive(true);
            
            yield return new WaitForSeconds(7);
            inventory.AddToInventory(objectToGive);
            AkSoundEngine.PostEvent("Play_Island" + currentIslandNumber + "_Object0", gameObject);
            yield return new WaitForSeconds(2.5f);
            EventManager.Instance.Raise(new DiscoverZoneEvent() { zoneNumber = nextZone });
            

            //StartCoroutine(ChangeScreenType(EScreenType.ISLAND_FULLSCREEN));
        }
        else
        {
            //Play Berth Animation
            upPartAnimator.SetTrigger("Berth");
            //StartCoroutine(ChangeScreenType(EScreenType.ISLAND_SMALL));
        }
            
    }

    public void LeaveIsland()
    {
        upPartAnimator.ResetTrigger("Berth");
        AkSoundEngine.PostEvent("Stop_AMB_Island" + currentIslandNumber, gameObject);
        //StartCoroutine(ChangeScreenType(EScreenType.SEA));
        upPartAnimator.SetTrigger("LeaveIsland");
        currentIslandNumber = -1;
    }

    IEnumerator ChangeScreenType(EScreenType newType)
    {
        screenType = newType;

        if (screenType == EScreenType.ISLAND_FULLSCREEN)
        {
            upPartAnimator.SetTrigger("Discover");
            //desk.SetActive(false);
            //telescope.SetActive(false);
            //islandScreen.SetActive(true);
            StartCoroutine(ChangeScreenType(EScreenType.ISLAND_SMALL));
        }
        else if (screenType == EScreenType.ISLAND_SMALL)
        {
            //desk.SetActive(true);
            //telescope.SetActive(false);
            //islandScreen.SetActive(true);
            if (firstVisit)
            {
                yield return new WaitForSeconds(7);
                inventory.AddToInventory(objectToGive);
                AkSoundEngine.PostEvent("Play_Island" + currentIslandNumber + "_Object0", gameObject);
                yield return new WaitForSeconds(2.5f);
                EventManager.Instance.Raise(new DiscoverZoneEvent() { zoneNumber = nextZone });
            }
            else
                EventManager.Instance.Raise(new BlockInputEvent() { block = false });
        }
        else
        {
            islandScreen.SetActive(false);
            telescopeScreen.SetActive(true);
        }
    }
}
