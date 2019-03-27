using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EScreenType { SEA, ISLAND, ISLAND_MIXED }

public enum EIslandIlluType { BIG, SMALL, COUNT }

public class ScreenManager : MonoBehaviour
{
    [SerializeField]
    GameObject telescope;

    [SerializeField]
    GameObject desk;

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

    bool giveObject;
    GameObject objectToGive;

    public void Berth(Sprite illustration, Sprite character, bool firstTime, GameObject charaObject)
    {
        for (int i = 0; i < (int)EIslandIlluType.COUNT; i++)
        {
            islandIllustrations[i].GetComponent<SpriteRenderer>().sprite = illustration;
            islandCharacters[i].GetComponent<SpriteRenderer>().sprite = character;
        }

        giveObject = firstTime;
        objectToGive = charaObject;
        StartCoroutine(ChangeScreenType(EScreenType.ISLAND));
    }

    public void LeaveIsland()
    {
        StartCoroutine(ChangeScreenType(EScreenType.SEA));
    }

    IEnumerator ChangeScreenType(EScreenType newType)
    {
        screenType = newType;

        if (screenType == EScreenType.ISLAND)
        {
            desk.SetActive(false);
            telescope.SetActive(false);
            island.SetActive(true);
            islandIlluType = EIslandIlluType.BIG;
            ChangeIslandIlluType();
            yield return new WaitForSeconds(2);
            StartCoroutine(ChangeScreenType(EScreenType.ISLAND_MIXED));
        }
        else if (screenType == EScreenType.ISLAND_MIXED)
        {
            desk.SetActive(true);
            islandIlluType = EIslandIlluType.SMALL;
            ChangeIslandIlluType();
            if (giveObject)
            {
                Debug.Log("I'm giving you a little present :3");
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
