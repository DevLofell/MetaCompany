using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NutCracker_Kick : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Player와 충돌했는지 확인
        if (other.CompareTag("Player"))
        {
            // Nutcracker라는 자식 오브젝트에서 Animator 컴포넌트를 가져오기
            Transform nutcrackerTransform = transform.parent.Find("Nutcracker");
            if (nutcrackerTransform != null)
            {
                Animator animator = nutcrackerTransform.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger("Kick"); // 애니메이터 트리거 설정
                }
            }

            // Player의 체력을 0으로 설정
            PlayerMove playerHealth = other.GetComponent<PlayerMove>();

            if (playerHealth != null)
            {
                playerHealth.health = 0;
            }
        }
    }
}

