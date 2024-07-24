using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [Header("Boxcast Property")]
    [SerializeField] private Vector3 boxSize;
    [SerializeField] private LayerMask groundLayer;

    [Header("Debug")]
    [SerializeField] private bool drawGizmo;
    public bool IsGrounded()
    {
        Collider[] hits = Physics.OverlapBox(transform.position, boxSize / 2, transform.rotation, groundLayer);
        if (hits.Length > 0)
        {
            if (hits[0].CompareTag("Ground"))
            {
                return true;
            }
        }
        return false;
    }
}
