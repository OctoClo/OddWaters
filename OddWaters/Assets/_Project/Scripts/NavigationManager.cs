using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    [SerializeField]
    Map map;

    [SerializeField]
    GameObject boat;
    float boatPosY;

    [SerializeField]
    Telescope telescope;

    int currentZone = -1;

    void Start()
    {
        boatPosY = boat.transform.position.y;
    }

    void Update()
    {
        if (map.currentZone != currentZone)
        {
            currentZone = map.currentZone;
            Sprite newSprite = map.GetCurrentZoneSprite();
            StartCoroutine(telescope.ChangeSprite(newSprite));
        }
    }

    public void NavigateTo(Vector3 targetPos)
    {
        if ((targetPos - boat.transform.position).magnitude >= 0.5f)
        {
            targetPos.y = boatPosY;
            boat.transform.position = targetPos;
        }
    }
}
