using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{

    // characterController 컴포넌트
    public CharacterController cc;

    // 이동속도 (이거를 수업 예제에서는 y 방향 속력인가? 정답은 아니다. 이동 속도랑 y방향 속력은 아예 달라서 전역 변수를 하나 만들어줘야 한다.)
    public float speed = 5.0f;

    // 중력 (캐릭터 컨트롤러에서는 중력이 필요하구나)
    public float gravity = -9.81f;

    // 점프힘
    public float jumForce = 5f;

    // y방향 속력(점프 속력)
    float yVelocity;

    // 현재 점프 횟수
    int jumpCnt;


    // 오디오 소스 컴포넌트
    public AudioSource audioSource;

    // 현재 시간
    float currTime;

    void Start()
    {

    }


    void Update()
    {
        // 현재 캐릭터가 땅에 있는가?
        if (cc.isGrounded)
        {
            // 만약에 땅에 있다면 yVelocity를 초기화
            yVelocity = 0; // Velocity 의 단어는 '속도'이다.
            jumpCnt = 0;
        }

        // 점프를 할 때는 캐릭터 y축에다가 힘을 주기

        if (Input.GetButtonDown("Jump"))
        {
            jumpCnt += 1;
            // 점프는 딱 한 번만
            if (jumpCnt == 1)
            {
                yVelocity = jumForce;
                jumpCnt = 0;
            }
        }
        else
        {
            // 점프 중이 아닐 때는 중력의 영향을 받기
            yVelocity += gravity * Time.deltaTime;
        }


        /*
         
        // 입력 받아서 이동 벡터로 설정
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 dir = transform.transformdirection(moveX, yVelocity, movez);
        dir.Normalize();

        dir.y = yVelocity;


        // 이동하기
        cc.Move(dir * speed * Time.deltaTime);


        내가 틀린 코드
        */

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Vector3 move = new Vector3(moveX, yVelocity, moveZ);

        // 이것도 되나 한 번 실험해보기
        Vector3 dir = transform.TransformDirection(moveX, yVelocity, moveZ);

        // 로컬 좌표계에서 월드 좌표계로 변환??
        // Vector3 dir = transform.TransformDirection(move);

        dir.Normalize();

        dir.y = yVelocity; // dir은 벡터의 방향

        // 이동하기
        cc.Move(dir * speed * Time.deltaTime);

        if(dir.magnitude > 0.01f)
        {
            currTime += Time.deltaTime;
            if(currTime > 1)
            {
                audioSource.Play();
                currTime = 0;
            }
        }
        else
        {
            audioSource.Pause();
        }

        

    }



}