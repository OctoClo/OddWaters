using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    List<Vector3> spawnPositions;
    int objectCounter;

    public void AddToInventory(GameObject prefab)
    {
        GameObject newObject = Instantiate(prefab, transform);
        newObject.transform.position = spawnPositions[objectCounter];
        objectCounter++;
    }
}
