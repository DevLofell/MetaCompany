using UnityEngine;
using UnityEditor;

public class ItemSynchronizer : EditorWindow
{
    [MenuItem("Tools/Synchronize Item Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<ItemSynchronizer>("Item Synchronizer");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Synchronize All Items"))
        {
            SynchronizeItems();
        }
    }

    private void SynchronizeItems()
    {
        string[] guids = AssetDatabase.FindAssets("t:Item");
        Debug.Log($"Found {guids.Length} Item ScriptableObjects");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Item itemData = AssetDatabase.LoadAssetAtPath<Item>(path);

            if (itemData == null)
            {
                Debug.LogError($"Failed to load Item ScriptableObject at path: {path}");
                continue;
            }

            Debug.Log($"Processing Item: {itemData.Name}, ID: {itemData.ID}, Type: {itemData.Type}");

            string prefabPath = path.Replace(".asset", ".prefab");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"Prefab not found for item: {itemData.Name}. Expected at {prefabPath}");
                continue;
            }

            ItemComponent itemComponent = prefab.GetComponent<ItemComponent>();
            if (itemComponent == null)
            {
                itemComponent = prefab.AddComponent<ItemComponent>();
                Debug.Log($"Added ItemComponent to prefab: {itemData.Name}");
            }

            itemComponent.itemData = itemData;
            itemComponent.SyncWithData();

            EditorUtility.SetDirty(itemComponent);
            EditorUtility.SetDirty(prefab);

            bool savedSuccessfully = PrefabUtility.SavePrefabAsset(prefab);
            if (savedSuccessfully)
            {
                Debug.Log($"Synchronized and saved prefab: {itemData.Name}, Type: {itemComponent.type}, kg: {itemComponent.kg}");
            }
            else
            {
                Debug.LogError($"Failed to save prefab: {itemData.Name}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}