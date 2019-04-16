using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EScreenType { SEA, ISLAND_FULLSCREEN, ISLAND_SMALL }

public enum EIslandIlluType { FULLSCREEN, SMALL, COUNT }

public class ScreenManager : MonoBehaviour
{
    [SerializeField]
    GameObject telescope;

    [SerializeField]
    GameObject desk;

    [SerializeField]
    Inventory inventory;

    [SerializeField]
    GameObject island;
    [SerializeField]
    GameObject[] islandFolders;
    [SerializeField]
    GameObject[] islandIllustrations;
    [SerializeField]
    GameObject[] islandCharacters;

    [HideInInspector]
    public EScreenType screenType = EScreenType.SEA;

    EIslandIlluType islandIlluType;

    Island currentIsland;
    bool firstVisit;
    GameObject objectToGive;
    int nextZone;

    private void Start()
    {
        AkSoundEngine.SetState("SeaIntensity", "CalmSea");
        AkSoundEngine.SetState("Weather", "Fine");
        AkSoundEngine.PostEvent("Play_AMB_Sea", gameObject);
    }

    public void Berth(Island island)
    {
        for (int i = 0; i < (int)EIslandIlluType.COUNT; i++)
        {
            islandIllustrations[i].GetComponent<SpriteRenderer>().sprite = island.illustration;
            islandCharacters[i].GetComponent<SpriteRenderer>().sprite = island.character;
        }

        currentIsland = island;
        firstVisit = island.firstTimeVisiting;
        if (firstVisit)
        {
            objectToGive = island.objectToGive;
            nextZone = island.nextZone;
            StartCoroutine(ChangeScreenType(EScreenType.ISLAND_FULLSCREEN));
        }
        else
            StartCoroutine(ChangeScreenType(EScreenType.ISLAND_SMALL));
    }

    public void LeaveIsland()
    {
        AkSoundEngine.PostEvent("Stop_AMB_Island" + currentIsland.islandNumber, gameObject);
        StartCoroutine(ChangeScreenType(EScreenType.SEA));
    }

    IEnumerator ChangeScreenType(EScreenType newType)
    {
        screenType = newType;

        if (screenType == EScreenType.ISLAND_FULLSCREEN)
        {
            desk.SetActive(false);
            telescope.SetActive(false);
            island.SetActive(true);
            islandIlluType = EIslandIlluType.FULLSCREEN;
            ChangeIslandIlluType();
            yield return new WaitForSeconds(2);
            StartCoroutine(ChangeScreenType(EScreenType.ISLAND_SMALL));
        }
        else if (screenType == EScreenType.ISLAND_SMALL)
        {
            desk.SetActive(true);
            telescope.SetActive(false);
            island.SetActive(true);
            islandIlluType = EIslandIlluType.SMALL;
            ChangeIslandIlluType();
            if (firstVisit)
            {
                yield return new WaitForSeconds(0.5f);
                inventory.AddToInventory(objectToGive);
                AkSoundEngine.PostEvent("Play_Island" + currentIsland.islandNumber + "_Object0", gameObject);
                yield return new WaitForSeconds(1f);
                EventManager.Instance.Raise(new DiscoverZoneEvent() { zoneNumber = nextZone });
            }
            else
                EventManager.Instance.Raise(new BlockInputEvent() { block = false });
        }
        else
        {
            island.SetActive(false);
            telescope.SetActive(true);
        }
    }

    void ChangeIslandIlluType()
    {
        int otherType = ((int)islandIlluType + 1) % (int)EIslandIlluType.COUNT;

        islandFolders[(int)islandIlluType].SetActive(true);
        islandIllustrations[(int)islandIlluType].SetActive(true);
        islandCharacters[(int)islandIlluType].SetActive(true);

        islandFolders[otherType].SetActive(false);
        islandIllustrations[otherType].SetActive(false);
        islandCharacters[otherType].SetActive(false);
    }
}
