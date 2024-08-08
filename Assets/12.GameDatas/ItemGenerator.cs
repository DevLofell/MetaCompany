using System.IO;
using UnityEditor;
using UnityEngine;

public class ItemGenerator : EditorWindow
{
    private MainItemDatabase mainDatabase;

    [MenuItem("Tools/Item Generator/Generate Item ScriptableObjects")]
    public static void ShowWindow()
    {
        GetWindow<ItemGenerator>("Item Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Item ScriptableObject Generator", EditorStyles.boldLabel);

        mainDatabase = (MainItemDatabase)EditorGUILayout.ObjectField("Main Item Database", mainDatabase, typeof(MainItemDatabase), false);

        if (GUILayout.Button("Generate Items"))
        {
            if (mainDatabase == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign the Main Item Database", "OK");
                return;
            }
            GenerateItems();
        }
    }

    private void GenerateItems()
    {
        string folderPath = "Assets/ScriptableObjects/Items";

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string guid = AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Items");
            folderPath = AssetDatabase.GUIDToAssetPath(guid);
        }

        foreach (var itemData in mainDatabase.items)
        {
            Item newItem = ScriptableObject.CreateInstance<Item>();
            newItem.ID = itemData.ID;
            newItem.Name = itemData.Name;
            newItem.Type = itemData.Type;
            newItem.kg = itemData.kg;
            newItem.hand = itemData.hand;
            newItem.sound = itemData.sound;
            newItem.interact = itemData.interact;
            newItem.Conduction = itemData.Conduction;

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folderPath, $"{itemData.Name}.asset"));
            AssetDatabase.CreateAsset(newItem, assetPath);
            Debug.Log($"Created Item ScriptableObject: {itemData.Name} at {assetPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", "Item ScriptableObjects generated successfully!", "OK");
    }
}