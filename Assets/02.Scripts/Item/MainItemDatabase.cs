using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Main Item Database", menuName = "Inventory System/Main Item Database")]
public class MainItemDatabase : ScriptableObject
{
    public List<MainItemData> items = new List<MainItemData>();

    public MainItemData GetItemById(int id)
    {
        return items.Find(item => item.ID == id);
    }
}