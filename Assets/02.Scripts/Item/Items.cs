using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
{
    // 아이템 기본 속성
    public int ID { get; private set; }
    public string Name { get; private set; }
    public string Type { get; private set; }
    public float Kg { get; private set; }
    public string Hand { get; private set; }
    public bool HasSound { get; private set; }
    public bool IsInteractable { get; private set; }
    public bool HasConduction { get; private set; }

    // 추가적인 게임플레이 관련 속성
    private bool isPickedUp = false;

    // 아이템 데이터로 초기화하는 메서드
    public void InitializeFromData(MainItemData data)
    {
        ID = data.ID;
        Name = data.Name;
        Type = data.Type;
        Kg = data.kg;
        Hand = data.hand;
        HasSound = data.sound;
        IsInteractable = data.interact;
        HasConduction = data.Conduction;
    }

    // 아이템을 집어들었을 때 호출되는 메서드
    public void PickUp()
    {
        isPickedUp = true;
    }

    // 아이템을 떨어뜨렸을 때 호출되는 메서드
    public void Drop()
    {
        isPickedUp = false;
    }

    // 아이템 사용 메서드 (상호작용 가능한 아이템의 경우)
    public void Use()
    {
        if (IsInteractable)
        {
            // 아이템 사용 로직 구현
        }
    }

    // 아이템이 소리를 내는 경우의 메서드
    public void PlaySound()
    {
        if (HasSound)
        {
            // 소리 재생 로직 구현
        }
    }
}

