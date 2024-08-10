using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MainItemManager : MonoBehaviour
{
    public MainItemDatabase mainItemDatabase;

    void Start()
    {
        MainItemData item = mainItemDatabase.GetItemById(10001);
        Debug.Log($"Item name: {item.Name}, Weight: {item.kg}kg");
    }
}
