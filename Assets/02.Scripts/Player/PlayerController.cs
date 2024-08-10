using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float playerSpeed = 2f;
    private float originSpeed;
    [SerializeField]
    private float jumpForce = 2f;
    [SerializeField]
    private float gravity = -9.81f;
    [SerializeField]
    private float rotationSpeed = 0.001f;

    private GroundCheck groundCheck;
    private CharacterController cc;
    private Vector3 playerVelocity;
    private bool isGroundedPlayer;
    private bool isSlopePlayer;
    private InputManager inputManager;
    private Transform cameraTr;

    private PlayerAnimation anim;
    private StaminaSystem stamina;
    private PlayerSoundSystem sound;

    public GameObject playerModel;
    private void Start()
    {
        groundCheck = GetComponent<GroundCheck>();
        cc = GetComponent<CharacterController>();
        inputManager = InputManager.instance;
        cameraTr = Camera.main.transform;
        jumpVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        originSpeed = playerSpeed;
        anim = GetComponent<PlayerAnimation>();
        stamina = GetComponent<StaminaSystem>();
        sound = GetComponentInChildren<PlayerSoundSystem>();
    }
    private float gravityMultiplier = 2f; // 중력 배율 추가
    private float jumpVelocity;
    private bool isJumpOnce = false;
    public bool isLandingOnce = false;

    private void Update()
    {
        if (inputManager.IsInputEnabled())
        {
            isGroundedPlayer = groundCheck.IsGrounded();
            isSlopePlayer = groundCheck.IsSlope();

            // 입력 및 이동 방향 계산
            Vector2 movement = inputManager.GetPlayerMovement();
            Vector3 moveDirection = Vector3.ProjectOnPlane(cameraTr.TransformDirection(new Vector3(movement.x, 0f, movement.y)), Vector3.up).normalized;
            moveDirection.y = 0f;
            if (isCrouch == false)
            {
                PlayerWalk(movement);
            }
            PlayerRun();
            /*if ()
            {
                PlayerAttack();
            }*/
            
            PlayerCrouching();

            // 점프 처리
            if (!stamina.isImpossibleJump)
            {
                if (inputManager.PlayerJumpedThisFrame() && ((isGroundedPlayer || isSlopePlayer) && !isCrouch))
                {
                    PlayerJump();
                    isJumpOnce = true;
                    anim.OnStand();
                    stamina.ChangeCoroutine("Increase");
                }
            }
            
            //Quaternion targetRotation = Quaternion.LookRotation();
            //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 30f);
            
            // 회전 처리
            //Vector3 lookDirection = cameraTr.forward;
            //lookDirection.y = 0f;
            //if (lookDirection.magnitude > 0.1f)
            //{
            //    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            //    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 30f);
            //}

            // 지면에 닿았을 때 y 속도 리셋
            if (isGroundedPlayer && playerVelocity.y < 0)
            {
                if (!isLandingOnce)
                {
                    if (inputManager.PlayerRan())
                    {
                        stamina.ChangeCoroutine("Decrease");
                    }
                    isLandingOnce = true;
                    sound.StartLanding();
                }
                if (isJumpOnce == true)
                {
                    anim.EndJump();
                    cc.height = 1.8f;
                    cc.center = new Vector3(0f, 0.88f, 0f);
                    isJumpOnce = false;
                }
                playerVelocity.y = -5f;
            }
            else if (!isGroundedPlayer && playerVelocity.y != 0)
            {
                isLandingOnce = false;
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
                // 일반 지면 이동
                if (isGroundedPlayer)
                {
                    playerVelocity.x = moveDirection.x * playerSpeed;
                    playerVelocity.z = moveDirection.z * playerSpeed;
                }

                // 중력 적용
                playerVelocity.y += gravity * gravityMultiplier * Time.deltaTime;
            }
            if (moveDirection.magnitude > 1f)
            {
                moveDirection.Normalize();
            }
            // 캐릭터 이동
            cc.Move(playerVelocity * Time.deltaTime);
        }
        else
        {
            StopWalkAnimations();
        }
        
        // 좌우 회전
        if (inputManager.isRotateAble)
        {
            Vector2 lookInput = inputManager.GetMouseDelta();
            float mouseX = lookInput.x * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up * mouseX);
        }
    }

    private void PlayerJump()
    {
        stamina.DecreaseStaminaForJump();
        anim.OnAir();
        // 현재 수직 속도를 고려하여 점프 힘 조절
        float currentYVelocity = playerVelocity.y;
        float adjustedJumpVelocity = Mathf.Sqrt(jumpVelocity * jumpVelocity - 2f * currentYVelocity * currentYVelocity);

        cc.height = 1.65f;
        //cc.center = new Vector3(0f, 1f, 0f);

        playerVelocity.y = adjustedJumpVelocity;
        isGroundedPlayer = false;
        isSlopePlayer = false;
    }

    private void PlayerWalk(Vector2 movement)
    {
        float deadzone = 0.1f; // 데드존 값 설정
        bool isMoving = movement.magnitude > deadzone;

        if (!isMoving)
        {
            StopWalkAnimations();
            return;
        }

        // 수직 이동 우선 처리
        if (movement.y != 0f)
        {
            StopWalkAnimations();
            if (movement.y > 0f)
            {
                anim.OnWalk(true);
            }
            else // movement.y < 0f
            {
                anim.OnWalkBack(true);
            }
        }
        // 수직 이동이 없을 때만 수평 이동 처리
        else if (movement.x != 0f)
        {
            StopWalkAnimations();
            if (movement.x > 0f)
            {
                anim.OnSideWalkR();
            }
            else // movement.x < 0f
            {
                anim.OnSideWalkL();
            }
        }
    }

    private void StopWalkAnimations()
    {
        anim.OnWalk(false);
        anim.OnWalkBack(false);
        anim.OnSideEnd();
    }
    bool runDecreaseCount = false;
    private void PlayerRun()
    {
        // 달리기 처리
        if (!stamina.isImpossibleJump && inputManager.PlayerRan())
        {
            if (runDecreaseCount == false)
            {
                runDecreaseCount = true;
                stamina.ChangeCoroutine("Decrease");
            }
            anim.OnStand();
            playerSpeed = 5f;
            anim.OnRun(true);
            isCrouch = false;
        }
        else
        {
            if (inputManager.PlayerRunReleasedOnce())
            {
                runDecreaseCount = false;
                stamina.ChangeCoroutine("Increase");
            }
            else if (inputManager.PlayerRan())
            {
                stamina.ChangeCoroutine("Decrease");
            }
            playerSpeed = originSpeed;
            anim.OnRun(false);
        }
    }
    public bool isCrouch = false;
    private void PlayerCrouching()
    {
        if (isJumpOnce == false)
        {
            if (inputManager.PlayerCrouchinged() && isCrouch == false)
            {
                inputManager.ToggleCrouchinged();
                playerSpeed = 1f;
                isCrouch = true;
                anim.OnWalk(false);
                cc.height = 0.9f;
                cc.center = new Vector3(0f, 0.44f, 0f);
                anim.OnCrouching();
            }
            else if(inputManager.PlayerCrouchinged() && isCrouch == true)
            {
                inputManager.ToggleCrouchinged();
                playerSpeed = originSpeed;
                isCrouch = false;
                cc.height = 1.8f;
                cc.center = new Vector3(0f, 0.88f, 0f);
                anim.OnStand();
            }
            if (isCrouch == true)
            {
                if (inputManager.GetPlayerMovement().y > 0 || (inputManager.GetPlayerMovement().x != 0))
                {
                    anim.CrouchingMove(1);
                }
                else if (inputManager.GetPlayerMovement().y < 0)
                {
                    anim.CrouchingMove(-1);
                }
                else
                {
                    anim.CrouchingMove(0);
                }
            }
        }
    }

    private void PlayerAttack()
    {
        if (inputManager.PlayerAttackStarted())
        {
            anim.IsAttckReady();
        }
        else if (inputManager.PlayerAttackImacted())
        {
            anim.isAttackImpact();
        }
    }
}
