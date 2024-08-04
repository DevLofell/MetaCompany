using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static readonly string SHIP_NAME = "01.Ship";
    public Button startGameButton;
    public Button gameQuitButton;
    // Start is called before the first frame update
    void Start()
    {
        startGameButton.onClick.AddListener(GameStartToShip);
        gameQuitButton.onClick.AddListener(GameQuit);
    }

    private void OnDestroy()
    {
        startGameButton.onClick.RemoveListener(GameStartToShip);
        gameQuitButton.onClick.RemoveListener(GameQuit);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void GameStartToShip()
    {
        SceneLoadManager.Instance.LoadSceneByName(SHIP_NAME);
    }
    public void GameQuit()
    {
        Application.Quit();
    }
}
