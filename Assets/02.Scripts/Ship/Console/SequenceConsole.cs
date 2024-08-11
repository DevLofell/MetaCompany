using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;
using UnityEngine.UI;


[System.Serializable]
public class Sequence 
{
    public string name;
    public GameObject prefab;
}
public class SequenceConsole : MonoBehaviour
{
    public static SequenceConsole instance;


    public GameObject spacerGameObject;
    
    public Sequence startSequence;
    public List<Sequence> commandList;

    public GameObject nowGameObject;
    public GameObject nowOutputGameObject;
    public InputField nowOutput;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
            return;
        }

        commandList = new List<Sequence>();
        
    }

    public void OnEnable()
    {
        
    }
    public void OnDestroy()
    {

    }

    /// <summary>
    /// 커멘드 화면에 보이기
    /// </summary>
    private void LoadAndDisplayStartScreen()
    {

    }
    /// <summary>
    /// 모니터 초기화
    /// </summary>
    /// <param name="message">초기화 할 내용</param>
    public void PrintToConsole(string message, bool isClear = true)
    {
        

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
