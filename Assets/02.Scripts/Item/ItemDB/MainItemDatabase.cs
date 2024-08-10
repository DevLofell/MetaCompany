using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class ItemsData
{
    public int ID;
    public string Name;
    public string Type;
    public int kg;
    public string hand;
    public bool sound;
    public bool interact;
    public bool Conduction;
}

[CreateAssetMenu(fileName = "MainItemDatabase", menuName = "Inventory/MainItemDatabase")]
public class MainItemDatabase : ScriptableObject
{
    public List<MainItemData> items = new List<MainItemData>();

    public MainItemData GetItemById(int id)
    {
        return items.Find(item => item.ID == id);
    }
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public int ID;
    public string Name;
    public string Type;
    public int kg;
    public string hand;
    public bool sound;
    public bool interact;
    public bool Conduction;
}