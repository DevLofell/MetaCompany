using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] public List<GameObject> inventory = new List<GameObject>();
    private InputManager inputManager;
    private int curInventoryContainerNum = 0;
    [SerializeField] private float selectScale = 0.7f;
    [SerializeField] private float normalScale = 0.55f;
    [SerializeField] private float scrollingDelay = 3f;
    private WaitForSeconds waitScrollingDelay;
    private bool canChange = true;
    private Coroutine coroutine;

    private UIManager uiManager;
    private void Start()
    {
        uiManager = UIManager.instance;
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
                StartCoroutine(ScrollDelay(scrollValue.y));
            }
        }
    }
    private IEnumerator ScrollDelay(float scrollValue)
    {
        canScroll = false;
        Switching(scrollValue);
        yield return waitScrollingDelay;
        canScroll = true;
    }
    private void Switching(float scrollValue)
    {
        if (scrollValue > 0)
        {
            curInventoryContainerNum++;
        }
        else if (scrollValue < 0)
        {
            curInventoryContainerNum--;
        }
        curInventoryContainerNum = (curInventoryContainerNum+4) % 4;
        UIManager.instance.ResizeInventoryUI(curInventoryContainerNum);
    }

    public void PutIndexInventory(GameObject obj, Sprite icon)
    {
        if (obj == null)
        {
            print("inven");
        }
        else
        {
            inventory[curInventoryContainerNum] = obj.gameObject;
            UIManager.instance.PutInInventoryUI(curInventoryContainerNum, icon);
        }
    }

    

    //IEnumerator ScrollingTimer()
    //{
    //    yield return waitScrollingDelay;
    //    time = scrollingDelay;
    //}
}
