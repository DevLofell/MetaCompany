using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnWalk(bool isWalk)
    {
        animator.SetBool("isWalk", isWalk);
        animator.SetFloat("WalkFrontBack", 1.0f);
    }
    public void OnWalkBack(bool isWalk)
    {
        animator.SetBool("isWalk", isWalk);
        animator.SetFloat("WalkFrontBack", -1.0f);
    }
    public void OnRun(bool isRun)
    {
        animator.SetBool("isRun", isRun);
    }

    public void OnAir()
    {
        animator.SetBool("isJump", true);
        animator.Play("Jump");
    }

    public void EndJump()
    {
        animator.SetBool("isJump", false);
    }

    public void OnSideWalkR()
    {
        animator.SetBool("isSideR", true);
    }
    public void OnSideWalkL()
    {
        animator.SetBool("isSideL", true);
    }

    public void OnSideEnd()
    {
        animator.SetBool("isSideR", false);
        animator.SetBool("isSideL", false);
    }

    public void OnCrouching()
    {
        animator.SetBool("isCrouching", true);
    }

    public void CrouchingMove(int speed)
    {
        if (speed == 0)
        {
            animator.Play("Crouch", 0, 0);
        }
        else
        {
            animator.SetFloat("CrouchingFrontBack", speed);
        }
        
    }

    public void OnStand()
    {
        animator.SetBool("isCrouching", false);
    }
}
