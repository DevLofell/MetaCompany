using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // characterController 컴포넌트
    public CharacterController cc;

    // 이동속도
    public float speed = 5.0f;

    // 중력
    public float gravity = -9.81f;

    // 점프힘
    public float jumpForce = 5f;

    // y방향 속력(점프 속력)
    float yVelocity;

    // 오디오 소스 컴포넌트
    public AudioSource audioSource;

    // 현재 시간
    float currTime;

    // 플레이어 체력
    public float health = 100.0f;

    void Start()
    {
        yVelocity = 0;
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (cc != null && cc.enabled)
        {
            if (cc.isGrounded)
            {
                yVelocity = 0;

                if (Input.GetButtonDown("Jump"))
                {
                    yVelocity = jumpForce;
                }
            }
            else
            {
                yVelocity += gravity * Time.deltaTime;
            }

            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 dir = new Vector3(moveX, 0, moveZ);
            dir = transform.TransformDirection(dir);
            dir.Normalize();

            Vector3 move = dir * speed * Time.deltaTime;
            move.y = yVelocity * Time.deltaTime;
            cc.Move(move);

            if (dir.magnitude > 0.01f)
            {
                currTime += Time.deltaTime;
                if (currTime > 1)
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

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            health = 0;
            Destroy(gameObject, 1);
        }
    }
}
