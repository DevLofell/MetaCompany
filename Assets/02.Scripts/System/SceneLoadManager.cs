using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public enum SceneList : int
{
    Mainmenu = 0,
    Ship = 1,
    

}

public class SceneLoadManager : MonoSingleton<SceneLoadManager>
{
    public static SceneLoadManager Instance { get; private set; }

    [System.Serializable]
    public class SceneInfo
    {
        public string sceneName;
        public int buildIndex;
    }

    public List<SceneInfo> sceneList = new List<SceneInfo>();

    private void Awake()
    {
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

    public void LoadSceneByName(string sceneName)
    {
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