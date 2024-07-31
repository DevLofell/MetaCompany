using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    public Image staminaImage;

    private void Start()
    {
        staminaImage = GameObject.Find("StaminaGauge").GetComponent<Image>();
    }

    #region StaminaManage
    public void UpdateStaminaUI(float stamina)
    {
        staminaImage.fillAmount = stamina;
    }
    #endregion

    #region InventoryManage
    public void SelectInventoryUI(int num)
    {

    }
    #endregion
}
