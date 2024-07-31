using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
public class ItemData : CommandData
{
    public int price;
}
[System.Serializable]
public class ItemDataList
{
    public List<ItemData> regularItems;
    public List<ItemData> shipUpgrades;
    public List<ItemData> specialItems;
}

public class StoreManager : SubCommand
{
    private static readonly string STORE_FILE_PATH = Path.Combine(Application.streamingAssetsPath, "Data", "ItemList.json");

    public ItemDataList itemDataList;

    public override string GetFormattedList()
    {
        string regularItemsStr = FormatItems(itemDataList.regularItems);
        string shipUpgradesStr = FormatItems(itemDataList.shipUpgrades);
        string specialItemsStr = FormatItems(itemDataList.specialItems);

        return string.Format(ConsoleManager.instance.GetCommandDescription("store"), regularItemsStr, shipUpgradesStr, specialItemsStr);
    }
    private string FormatItems(List<ItemData> items)
    {
        string result = "";
        foreach (var item in items)
        {
            result += $"* {item.name}  //  가격: ${item.price}\n";
        }
        return result;
    }

    public override List<CommandData> LoadDataList()
    {
        List<CommandData> result = new List<CommandData>();
        if (File.Exists(STORE_FILE_PATH))
        {
            string jsonContent = File.ReadAllText(STORE_FILE_PATH, System.Text.Encoding.UTF8);
            itemDataList = JsonUtility.FromJson<ItemDataList>(jsonContent);
            result.AddRange(itemDataList.regularItems);
            result.AddRange(itemDataList.shipUpgrades);
            result.AddRange(itemDataList.specialItems);

        }
        else
        {
            Debug.LogError("ItemList.json not found!");
            itemDataList = new ItemDataList();
        }
        return result;
    }
}
