using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public UIManager uiManager;

    private void Awake()
    {
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
    }
}
