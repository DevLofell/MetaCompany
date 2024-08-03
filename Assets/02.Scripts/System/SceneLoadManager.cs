using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEditor.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum SceneList : int
{
    Mainmenu = 0,
    Ship = 1,
}

[System.Serializable]
public class SceneInfo
{
    public string sceneName;
    public int buildIndex;
#if UNITY_EDITOR
    public SceneAsset sceneAsset;
#endif
}

[ExecuteInEditMode]
public class SceneLoadManager : MonoSingleton<SceneLoadManager>
{
    public static SceneLoadManager Instance { get; private set; }

    public List<SceneInfo> sceneList = new List<SceneInfo>();

    private void Awake()
    {
        if (!Application.isPlaying)
        {
            if (Instance == null)
            {
                Instance = this;
            }
            return;
        }

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

#if UNITY_EDITOR
    public void LoadSceneInEditor(string sceneName)
    {
        SceneInfo sceneInfo = sceneList.Find(scene => scene.sceneName == sceneName);
        if (sceneInfo != null && sceneInfo.sceneAsset != null)
        {
            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneInfo.sceneAsset));
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' not found in the list or scene asset is missing.");
        }
    }
#endif

    public void LoadSceneByName(string sceneName)
    {
        if (!Application.isPlaying)
        {
#if UNITY_EDITOR
            LoadSceneInEditor(sceneName);
#endif
            return;
        }

        SceneInfo sceneInfo = sceneList.Find(scene => scene.sceneName == sceneName);
        if (sceneInfo != null)
        {
            StartCoroutine(LoadSceneAsync(sceneInfo.buildIndex));
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' not found in the list.");
        }
    }

    public void LoadSceneByIndex(int index)
    {
        if (!Application.isPlaying)
        {
#if UNITY_EDITOR
            if (index >= 0 && index < sceneList.Count)
            {
                LoadSceneInEditor(sceneList[index].sceneName);
            }
            else
            {
                Debug.LogError($"Scene index {index} is out of range.");
            }
#endif
            return;
        }

        if (index >= 0 && index < sceneList.Count)
        {
            StartCoroutine(LoadSceneAsync(sceneList[index].buildIndex));
        }
        else
        {
            Debug.LogError($"Scene index {index} is out of range.");
        }
    }

    private IEnumerator LoadSceneAsync(int buildIndex)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex);

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            Debug.Log($"Loading progress: {progress * 100}%");
            yield return null;
        }
    }
}
