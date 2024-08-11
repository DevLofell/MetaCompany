using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shovel : MonoBehaviour
{
    private BoxCollider col;
    private float time = 0f;

    private void Start()
    {
        col = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (InputManager.instance.PlayerAttackImacted())
        {
            col.enabled = true;
            StartCoroutine(Timer());
        }
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(0.3f);
        col.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            print("Enemy!");
            other.GetComponent<HpSystem>().UpdateHp(1f);
        }
    }
}
