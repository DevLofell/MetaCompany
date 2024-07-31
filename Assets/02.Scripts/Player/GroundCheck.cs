using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GroundCheck : MonoBehaviour
{
    [Header("Boxcast Property")]
    [SerializeField] private Vector3 boxSize;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField]
    private float maxSlopeAngle = 50f;
    private const float RAY_DISTANCE = 20f;
    private RaycastHit slopeHit;
    public Vector3 rayHitNormal;
    public bool isOnFlatGround;
    private InputManager inputManager;
    private Vector3 offsetRay = Vector3.zero;
    private void Start()
    {
        inputManager = InputManager.instance;
    }
    private void Update()
    {
        isOnFlatGround = CheckForFlatGround();
        
        Vector2 movement = inputManager.GetPlayerMovement();
        if (movement.y == 1f)
        {
            offsetRay += Vector3.forward;
        }
        if (movement.y == -1f)
        {
            offsetRay += Vector3.back;
        }
        if (movement.x == 1f)
        {
            offsetRay += Vector3.right;
        }
        if (movement.x == -1f)
        {
            offsetRay += Vector3.left;
        }
    }
    public bool IsGrounded()
    {
        Collider[] col = Physics.OverlapBox(transform.position, boxSize / 2, transform.rotation, groundLayer);
        if (col.Length > 0)
        {
            return true;
        }
        return false;
    }
    public bool IsSlope()
    {
        Ray ray = new(transform.position + offsetRay, Vector3.down);
        if (Physics.Raycast(ray, out slopeHit, RAY_DISTANCE, groundLayer))
        {
            rayHitNormal = slopeHit.normal;
            var angle = Vector3.Angle(Vector3.up, rayHitNormal);
            return angle != 0f && angle < maxSlopeAngle;
        }
        return false;
    }
    private bool CheckForFlatGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.1f))
        {
            return Vector3.Angle(hit.normal, Vector3.up) < 10f; // 5도 이하의 경사를 평지로 간주
        }
        return false;
    }
}
