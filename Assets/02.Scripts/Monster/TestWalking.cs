using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TestWalking : MonoBehaviour
{
        // Animator 컴포넌트를 저장할 전역 변수
        Animator animator;
    
    void Start()
    {
        // 현재 게임 오브젝트에서 Animator 컴포넌트를 찾음
        animator = GetComponent<Animator>();


    }

    
    void Update()
    {
        // 입력을 받아서 애니메이터 매개변수 변경 (예: 키보드 입력)
        if(Input.GetKeyDown(KeyCode.W))
        {
            // isWalking 매개변수를 true로 설정하여 walk 상태로 전환
            animator.SetBool("isWalking", true);
        } else if (Input.GetKeyUp(KeyCode.W))
        {
            // isWalking 매개변수를 false로 설정하여 idle 상태로 전환
            animator.SetBool("isWalking", false);
        }

    }
}
