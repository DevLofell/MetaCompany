using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpSystem : MonoBehaviour
{
    private float curHp;
    [SerializeField] private float maxHp;

    private void Start()
    {
        curHp = maxHp;
    }

    public void UpdateHp(float value)
    {
        curHp -= value;
        if (curHp > maxHp)
        {
            curHp = maxHp;
        }
        else if (curHp <= 0)
        {
            curHp = 0;
        }
    }
}
