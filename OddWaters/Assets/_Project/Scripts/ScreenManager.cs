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

    bool firstVisit;
    GameObject objectToGive;

    public void Berth(Sprite illustration, Sprite character, bool first, GameObject charaObject)
    {
        for (int i = 0; i < (int)EIslandIlluType.COUNT; i++)
        {
            islandIllustrations[i].GetComponent<SpriteRenderer>().sprite = illustration;
            islandCharacters[i].GetComponent<SpriteRenderer>().sprite = character;
        }

        firstVisit = first;
        if (firstVisit)
        {
            objectToGive = charaObject;
            StartCoroutine(ChangeScreenType(EScreenType.ISLAND_FULLSCREEN));
        }
        else
            StartCoroutine(ChangeScreenType(EScreenType.ISLAND_SMALL));
    }

    public void LeaveIsland()
    {
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
            }
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
