using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    public bool startedJumping = false;
    public bool isRunning = false;
    public bool isSprinting = false;
    public bool isWalking = false;
    public bool isCrouching = false;
    public bool isSliding = false;
    public bool isGrounded = true;
    public bool isOnWall = false;
    public bool isOnCeiling = false;

    void Start()
    {
        animator = GetComponent<Animator>();   
    }

    void Update()
    {
        CheckAnimationState();
    }

    public void SetYVelocity(float y){
        animator.SetFloat(AnimationStrings.yVelocity, y);
    }

    public void SetXVelocity(float x){
        animator.SetFloat(AnimationStrings.xVelocity, x);
    }

    private void CheckAnimationState()
    {
        animator.SetBool(AnimationStrings.isRunning, isRunning);
        animator.SetBool(AnimationStrings.isWalking, isWalking);
        animator.SetBool(AnimationStrings.isCrouching, isCrouching);
        animator.SetBool(AnimationStrings.isSliding, isSliding);
        animator.SetBool(AnimationStrings.isGrounded, isGrounded);
        animator.SetBool(AnimationStrings.isOnWall, isOnWall);
        animator.SetBool(AnimationStrings.isOnCeiling, isOnCeiling);
        animator.SetBool(AnimationStrings.isSprinting, isSprinting);
        
        if (startedJumping)
        {
            animator.SetTrigger(AnimationStrings.jump);
            startedJumping = false;
        }
    }

}
