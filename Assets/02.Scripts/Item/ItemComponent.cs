using UnityEngine;

public class ItemComponent : MonoBehaviour
{
    public Item itemData;
    public string type;
    public float kg;
    public string hand;
    public bool hasSound;
    public bool isInteractable;
    public bool hasConduction;

    private void OnValidate()
    {
        SyncWithData();
    }

    private void Awake()
    {
        SyncWithData();
    }

    public void SyncWithData()
    {
        if (itemData != null)
        {
            gameObject.name = itemData.Name;
            type = itemData.Type;
            kg = itemData.kg;
            hand = itemData.hand;
            hasSound = itemData.sound;
            isInteractable = itemData.interact;
            hasConduction = itemData.Conduction;
            Debug.Log($"Synced item: {itemData.Name}, ID: {itemData.ID}, Type: {type}, kg: {kg}");
        }
        else
        {
            Debug.LogWarning("Item data is not assigned for " + gameObject.name);
        }
    }

    private void OnEnable()
    {
        Debug.Log($"ItemComponent enabled for {gameObject.name}. ItemData: {(itemData != null ? itemData.Name : "null")}");
    }
}