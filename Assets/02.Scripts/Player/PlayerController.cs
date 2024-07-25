using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float playerSpeed = 2f;
    [SerializeField]
    private float jumpForce = 2f;
    [SerializeField]
    private float gravity = -9.81f;

    private GroundCheck groundCheck;
    private CharacterController cc;
    private Vector3 playerVelocity;
    private bool isGroundedPlayer;
    private bool isSlopePlayer;
    private InputManager inputManager;
    private Transform cameraTr;

    public GameObject playerModel;
    private void Start()
    {
        groundCheck = GetComponent<GroundCheck>();
        cc = GetComponent<CharacterController>();
        inputManager = InputManager.Instance;
        cameraTr = Camera.main.transform;
        jumpVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
    }
    private float gravityMultiplier = 2f; // 중력 배율 추가
    private float jumpVelocity;

    private void Update()
    {
        isGroundedPlayer = groundCheck.IsGrounded();
        isSlopePlayer = groundCheck.IsSlope();

        // 입력 및 이동 방향 계산
        Vector2 movement = inputManager.GetPlayerMovement();
        Vector3 moveDirection = Vector3.ProjectOnPlane(cameraTr.TransformDirection(new Vector3(movement.x, 0f, movement.y)), Vector3.up).normalized;
        moveDirection.y = 0f;
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // 점프 처리
        if (inputManager.PlayerJumpedThisFrame() && (isGroundedPlayer || isSlopePlayer))
        {
            PerformJump();
        }

        // 회전 처리
        Vector3 lookDirection = cameraTr.forward;
        lookDirection.y = 0f;
        if (lookDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 30f);
        }
        // 지면에 닿았을 때 y 속도 리셋
        if (isGroundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -5f;
        }
        // 경사면 및 중력 처리
        if (isSlopePlayer && !inputManager.PlayerJumpedThisFrame())
        {
            // 경사면에서의 이동
            Vector3 slopeMovement = Vector3.ProjectOnPlane(moveDirection, groundCheck.rayHitNormal).normalized;
            playerVelocity = slopeMovement * playerSpeed;
        }
        else
        {
            // 일반 지면 또는 공중에서의 이동
            playerVelocity.x = moveDirection.x * playerSpeed;
            playerVelocity.z = moveDirection.z * playerSpeed;

            // 중력 적용
            playerVelocity.y += gravity * gravityMultiplier * Time.deltaTime;
        }
        // 캐릭터 이동
        cc.Move(playerVelocity * Time.deltaTime);


    }

    private void PerformJump()
    {
        // 현재 수직 속도를 고려하여 점프 힘 조절
        float currentYVelocity = playerVelocity.y;
        float adjustedJumpVelocity = Mathf.Sqrt(jumpVelocity * jumpVelocity - 2f * currentYVelocity * currentYVelocity);

        playerVelocity.y = adjustedJumpVelocity;
        isGroundedPlayer = false;
        isSlopePlayer = false;
    }
}
