using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    
    [SerializeField] private GameObject[] itemList;
    [SerializeField] private GameObject[] playerItems;

    public bool isPlayerItems;
    public bool isRandom = false;
    public int playerItemIndex = 0;
    public int itemIndex = 0;

    private void Start()
    {
        if (isPlayerItems == true) 
        {
            SpawnPlayerItem(playerItemIndex);
        }
        else
        {
            SpawnItem(itemIndex);
        }
    }

    private void SpawnPlayerItem(int index)
    {
        Instantiate(playerItems[index], transform.position, transform.rotation);
    }

    private void SpawnItem(int index)
    {
        int randIndex = index;
        if (isRandom == true)
        {
            randIndex = Random.Range(0, itemList.Length);
        }

        Instantiate(itemList[randIndex], transform.position, transform.rotation);
    }
}
