#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(SceneLoadManager))]
public class SceneLoadManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SceneLoadManager manager = (SceneLoadManager)target;

        if (GUILayout.Button("Update Scene List"))
        {
            UpdateSceneList(manager);
        }
    }

    private void UpdateSceneList(SceneLoadManager manager)
    {
        manager.sceneList.Clear();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            manager.sceneList.Add(new SceneInfo { sceneName = sceneName, buildIndex = i, sceneAsset = sceneAsset });
        }

        EditorUtility.SetDirty(manager);
        Debug.Log("Scene list updated.");
    }
}
#endif
