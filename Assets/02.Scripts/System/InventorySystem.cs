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
    [SerializeField] private float scrollingDelay = 0.1f;
    private WaitForSeconds waitSrollingDelay;
    private Coroutine inventoryCoroutine;

    private void Start()
    {
        inputManager = InputManager.instance;
        waitSrollingDelay = new WaitForSeconds(scrollingDelay);
        inventoryCoroutine = StartCoroutine(Switching());
    }

    private void Update()
    {

    }
    IEnumerator Switching()
    {
        while (true)
        {
            yield return waitSrollingDelay;
            if (inputManager.IsScrollingEnter() && inputManager.InventorySwitching().y >= 1)
            {
                curInventoryContainerNum++;
            }
            else if (inputManager.IsScrollingEnter() && inputManager.InventorySwitching().y <= -1)
            {
                curInventoryContainerNum--;
            }
            print(Mathf.Abs(curInventoryContainerNum %= 4));
        }
    }
}
