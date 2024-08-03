using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    public Image staminaImage;
    public List<GameObject> inventoryUI = new List<GameObject>();
    public List<CanvasGroup> interactionUI = new List<CanvasGroup>();
    [SerializeField] private Vector2 sizeSelectInventory = new Vector2(70f, 70f);
    [SerializeField] private Vector2 sizeNormalInventory = new Vector2(55f, 55f);
    private int originIdx = 0;
    private float lerpSpeed = 7.5f; // 크기 변경 속도 조절
    private Coroutine[] resizeCoroutines;

    private void Start()
    {
        staminaImage = GameObject.Find("StaminaGauge").GetComponent<Image>();
        resizeCoroutines = new Coroutine[inventoryUI.Count];
        UpdateInteractionUI(0, 0, true);
    }

    #region StaminaManage
    public void UpdateStaminaUI(float stamina)
    {
        staminaImage.fillAmount = stamina;
    }
    #endregion

    #region InventoryManage
    public void UpdateInventoryUI(int idx)
    {
        if (originIdx != idx)
        {
            if (resizeCoroutines[originIdx] != null)
                StopCoroutine(resizeCoroutines[originIdx]);
            resizeCoroutines[originIdx] = StartCoroutine(ResizeUI(inventoryUI[originIdx].GetComponent<RectTransform>(), 
                inventoryUI[originIdx].GetComponent<RectTransform>().sizeDelta, sizeNormalInventory));

            originIdx = idx;
        }

        if (resizeCoroutines[idx] != null)
            StopCoroutine(resizeCoroutines[idx]);
        resizeCoroutines[idx] = StartCoroutine(ResizeUI(inventoryUI[idx].GetComponent<RectTransform>(), 
            sizeNormalInventory, sizeSelectInventory));
    }

    private IEnumerator ResizeUI(RectTransform rt, Vector2 startSize, Vector2 endSize)
    {
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            rt.sizeDelta = Vector2.Lerp(startSize, endSize, elapsedTime);
            elapsedTime += Time.deltaTime * lerpSpeed;
            yield return null;
        }
        rt.sizeDelta = endSize;
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
}
