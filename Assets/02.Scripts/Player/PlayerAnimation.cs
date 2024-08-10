using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private bool isEndAtk = true;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    
    private void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Attack_Impact") == true)
        {

            float animTime = animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
            if (animTime >= 1.0f)
            {
                isEndAtk = true;
                animator.SetBool("isAtkReady", false);
                animator.SetBool("isAtkImpacted", false);
            }
        }
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

    public void IsOneHand(bool isOneHand)
    {
        animator.SetBool("isOneHand", isOneHand);
    }

    public void IsTwoHand(bool isTwoHand)
    {
        animator.SetBool("isTwoHand", isTwoHand);
    }

    public void IsAttckReady()
    {
        if (isEndAtk == true)
        {
            isEndAtk = false;
            animator.SetBool("isAtkReady", true);
        }
        
    }
    public void isAttackImpact()
    {
        animator.SetBool("isAtkImpacted", true);
    }
}
