using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class Item
{
    public string name;
    public string description;
    public int price;
    public string prefabName;
    public string upgradeType;
    public string decorationType;
}

[System.Serializable]
public class ItemList
{
    public List<Item> regularItems;
    public List<Item> shipUpgrades;
    public List<Item> specialItems;
}

public class StoreManager : MonoBehaviour
{
    public ItemList itemList;

    void Start()
    {
        LoadItemList();
    }

    void LoadItemList()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath,"Data", "ItemList.json");
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            itemList = JsonUtility.FromJson<ItemList>(jsonContent);
        }
        else
        {
            Debug.LogError("ItemList.json not found!");
        }
    }

    public string GetFormattedItemList()
    {
        string regularItemsStr = FormatItems(itemList.regularItems);
        string shipUpgradesStr = FormatItems(itemList.shipUpgrades);
        string specialItemsStr = FormatItems(itemList.specialItems);

        return string.Format(ConsoleManager.instance.GetCommandDescription("store"),
                             regularItemsStr, shipUpgradesStr, specialItemsStr);
    }

    private string FormatItems(List<Item> items)
    {
        string result = "";
        foreach (var item in items)
        {
            result += $"* {item.name}  //  가격: ${item.price}\n";
        }
        return result;
    }

    public GameObject GetItemPrefab(string itemName)
    {
        Item item = FindItem(itemName);
        if (item != null && !string.IsNullOrEmpty(item.prefabName))
        {
            return Resources.Load<GameObject>(item.prefabName);
        }
        return null;
    }

    private Item FindItem(string itemName)
    {
        foreach (var item in itemList.regularItems)
        {
            if (item.name == itemName) return item;
        }
        foreach (var item in itemList.shipUpgrades)
        {
            if (item.name == itemName) return item;
        }
        foreach (var item in itemList.specialItems)
        {
            if (item.name == itemName) return item;
        }
        return null;
    }
}