using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class SceneReference
{
    [SerializeField] private Object sceneAsset;
    [SerializeField] private string sceneName = "";

    public string SceneName
    {
        get { return sceneName; }
    }

    public static implicit operator string(SceneReference sceneReference)
    {
        return sceneReference.SceneName;
    }
}


[CustomPropertyDrawer(typeof(SceneReference))]
public class SceneReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty sceneAsset = property.FindPropertyRelative("sceneAsset");
        SerializedProperty sceneName = property.FindPropertyRelative("sceneName");

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();

        Object asset = EditorGUI.ObjectField(position, label, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);

        if (EditorGUI.EndChangeCheck())
        {
            sceneAsset.objectReferenceValue = asset;
            if (asset != null)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                sceneName.stringValue = System.IO.Path.GetFileNameWithoutExtension(path);
            }
        }

        EditorGUI.EndProperty();
    }
}
