using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaSystem : MonoBehaviour
{
    private float curStamina = 0f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float increaseRatePerSecond = 10f;
    [SerializeField] private float decreaseRatePerSecond = 10f;
    [SerializeField] private float decreaseRateForJump = 10f;
    private WaitForSeconds increaseSec;
    private WaitForSeconds decreaseSec;
    private Coroutine staminaCoroutine;
    public float weight = 0f;

    private void Start()
    {
        curStamina = maxStamina;
        increaseSec = new WaitForSeconds(1f / increaseRatePerSecond);
        decreaseSec = new WaitForSeconds(1f / (decreaseRatePerSecond + weight));
        staminaCoroutine = StartCoroutine(IncreaseStamina());
    }

    public void UpdateStamina(float value)
    {
        curStamina += value;
        if (curStamina > maxStamina)
        {
            curStamina = maxStamina;
        }
        else if (curStamina <= 0)
        {
            curStamina = 0;
        }
        UIManager.instance.UpdateStaminaUI(curStamina / maxStamina);
    }

    IEnumerator IncreaseStamina()
    {
        while (true)
        {
            yield return increaseSec;
            UpdateStamina(1f);
        }
    }

    IEnumerator DecreaseStamina()
    {
        while (true)
        {
            yield return decreaseSec;
            UpdateStamina(-1f);
        }
    }

    public void DecreaseStaminaForJump()
    {
        UpdateStamina(-decreaseRateForJump);
    }

    public void ChangeCoroutine(string toName)
    {
        StopCoroutine(staminaCoroutine);
        switch (toName)
        {
            case "Increase":
                staminaCoroutine = StartCoroutine(IncreaseStamina());
                break;
            case "Decrease":
                staminaCoroutine = StartCoroutine(DecreaseStamina());
                break;
        }
    }
}
