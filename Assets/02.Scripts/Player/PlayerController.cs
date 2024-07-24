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
    private bool groundedPlayer;
    private InputManager inputManager;
    private Transform cameraTr;
    private void Start()
    {
        groundCheck = GetComponent<GroundCheck>();
        cc = GetComponent<CharacterController>();
        inputManager = InputManager.Instance;
        cameraTr = Camera.main.transform;
    }

    private void Update()
    {
        groundedPlayer = groundCheck.IsGrounded();
        if (groundedPlayer && playerVelocity.y <= 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 movement = inputManager.GetPlayerMovement();
        Vector3 dir = new Vector3(movement.x, 0f, movement.y);
        dir = cameraTr.forward * dir.z + cameraTr.right * dir.x;
        dir.y = 0f;
        if (dir.magnitude > 1f)
        {
            dir.Normalize();
        }

        cc.Move(dir * playerSpeed * Time.deltaTime);
        if (inputManager.PlayerJumpedThisFrame() && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpForce * -1f * gravity);
        }

        playerVelocity.y += gravity * Time.deltaTime;
        cc.Move(playerVelocity * Time.deltaTime);
    }
}
