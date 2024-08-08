using UnityEngine;
using UnityEditor;
using System.IO;

public class MainItemDatabaseCreator
{
    [MenuItem("Tools/Create Main Item Database")]
    public static void CreateMainItemDatabase()
    {
        MainItemDatabase asset = ScriptableObject.CreateInstance<MainItemDatabase>();

        AssetDatabase.CreateAsset(asset, "Assets/MainItemDatabase.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
}
