using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    public Image staminaImage;
    public List<GameObject> inventoryUI = new List<GameObject>();
    public List<GameObject> itemUIList = new List<GameObject>();
    public List<CanvasGroup> interactionUI = new List<CanvasGroup>();
    [SerializeField] private Vector3 sizeSelectInventory = new Vector3(1.25f, 1.25f, 1.25f);
    [SerializeField] private Vector3 sizeNormalInventory = new Vector3(1f, 1f, 1f);
    private int originIdx = 0;
    private float lerpSpeed = 7.5f; // 크기 변경 속도 조절
    private Coroutine[] resizeCoroutines;
    public Canvas canvas;

    private void Start()
    {
        resizeCoroutines = new Coroutine[inventoryUI.Count];
        staminaImage = GameObject.Find("StaminaGauge").GetComponent<Image>();
        
        UpdateInteractionUI(0, 0, true);
    }

    #region StaminaManage
    public void UpdateStaminaUI(float stamina)
    {
        staminaImage.fillAmount = stamina;
    }
    #endregion

    #region InventoryManage
    public void ResizeInventoryUI(int idx)
    {
        if (originIdx != idx)
        {
            if (resizeCoroutines[originIdx] != null)
                StopCoroutine(resizeCoroutines[originIdx]);
            resizeCoroutines[originIdx] = StartCoroutine(ResizeUI(inventoryUI[originIdx].GetComponent<RectTransform>(), 
                inventoryUI[originIdx].GetComponent<RectTransform>().localScale, sizeNormalInventory));

            originIdx = idx;
        }

        if (resizeCoroutines[idx] != null)
            StopCoroutine(resizeCoroutines[idx]);
        resizeCoroutines[idx] = StartCoroutine(ResizeUI(inventoryUI[idx].GetComponent<RectTransform>(), 
            sizeNormalInventory, sizeSelectInventory));
    }

    private IEnumerator ResizeUI(RectTransform rt, Vector3 startSize, Vector3 endSize)
    {
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            rt.localScale = Vector3.Lerp(startSize, endSize, elapsedTime);
            elapsedTime += Time.deltaTime * lerpSpeed;
            yield return null;
        }
        rt.localScale = endSize;
    }
    #endregion

    public void UpdateInteractionUI(int idx, int alpha, bool isAll)
    {
        if (isAll)
        {
            for (int i = 0; i < interactionUI.Count; i++)
            {
                interactionUI[i].GetComponent<CanvasGroup>().alpha = alpha;
            }
        }
        else
        {
            interactionUI[idx].GetComponent<CanvasGroup>().alpha = alpha;
        }
    }

    public void PutInInventoryUI(int idx, Sprite sprite)
    {
        if (sprite == null)
        {
            print("UI");
        }
        else
        {
            GameObject slotObject = inventoryUI[idx];
            Image slotImage = slotObject.GetComponentInChildren<Image>();

            GameObject imageObject = new GameObject("ItemImage");
            itemUIList[idx] = imageObject;
            imageObject.transform.SetParent(inventoryUI[idx].transform, false);
            slotImage = imageObject.AddComponent<Image>();
            slotImage.rectTransform.sizeDelta = new Vector2(30f, 30f);
            slotImage.sprite = sprite;
        }
    }

    public void PullOutInventoryUI(int idx)
    {
        if (itemUIList[idx] != null)
        {
            Destroy(itemUIList[idx]);
            itemUIList[idx] = null;
        }
    }
}
