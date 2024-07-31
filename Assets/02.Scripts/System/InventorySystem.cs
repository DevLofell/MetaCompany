using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private List<GameObject> inventory = new List<GameObject>();
    private InputManager inputManager;
    private int curInventoryContainerNum = 0;
    [SerializeField] private float selectScale = 0.7f;
    [SerializeField] private float normalScale = 0.55f;
    [SerializeField] private float scrollingDelay = 2f;
    private WaitForSeconds waitScrollingDelay;
    private bool canChange = true;
    private Coroutine coroutine;
    private void Start()
    {
        inputManager = InputManager.instance;
        waitScrollingDelay = new WaitForSeconds(scrollingDelay);
    }
    private bool canScroll = true;

    private void Update()
    {
        if (canScroll && inputManager.IsScrollingEnter())
        {
            Vector2 scrollValue = inputManager.InventorySwitching();
            if (scrollValue.y != 0)
            {
                StartCoroutine(ScrollDelay());
                Switching(scrollValue.y);
            }
        }
    }

    private void Switching(float scrollValue)
    {
        print("!!!");
        if (scrollValue > 0)
        {
            curInventoryContainerNum++;
        }
        else if (scrollValue < 0)
        {
            curInventoryContainerNum--;
        }

        curInventoryContainerNum = (curInventoryContainerNum + 4) % 4;
        Debug.Log($"Current Inventory Container: {curInventoryContainerNum}");
    }

    private IEnumerator ScrollDelay()
    {
        canScroll = false;
        yield return new WaitForSeconds(scrollingDelay);
        canScroll = true;
        
    }

    //IEnumerator ScrollingTimer()
    //{
    //    yield return waitScrollingDelay;
    //    time = scrollingDelay;
    //}
}
