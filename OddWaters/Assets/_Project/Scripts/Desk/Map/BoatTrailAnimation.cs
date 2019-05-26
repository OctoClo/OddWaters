using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatTrailAnimation : MonoBehaviour
{
    [SerializeField]
    float zOffset = 0.15f;
    [SerializeField]
    float animDuration = 0.2f;
    [SerializeField]
    float moveAmount = 0.02f;

    LineRenderer boatTrail;
    float nbMoves;
    float delayBetweenMoves;
    Vector3 pos;

    void Start()
    {
        boatTrail = GetComponent<LineRenderer>();
        nbMoves = zOffset / moveAmount;
        delayBetweenMoves = animDuration / nbMoves;
    }

    public void Hover()
    {
        StopAllCoroutines();
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        moveAmount *= -1;

        for (int move = 0; move < nbMoves; move++)
        {
            for (int i = 0; i < boatTrail.positionCount; i++)
            {
                pos = boatTrail.GetPosition(i);
                pos.z += moveAmount;
                boatTrail.SetPosition(i, pos);
            }

            yield return new WaitForSeconds(delayBetweenMoves);
        }
    }
}
