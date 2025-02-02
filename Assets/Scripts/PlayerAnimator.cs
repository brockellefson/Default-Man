using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    public bool startedJumping = false;
    public bool isRunning = false;
    public bool isMoving = false;
    public bool isCrouching = false;
    public bool isSliding = false;
    public bool isGrounded = true;
    public bool isOnWall;
    public bool isOnCeiling;

    void Start()
    {
        animator = GetComponent<Animator>();   
    }

    void LateUpdate()
    {
        CheckAnimationState();
    }

    public void SetYVelocity(float y){
        animator.SetFloat(AnimationStrings.yVelocity, y);
    }

    private void CheckAnimationState()
    {
        animator.SetBool(AnimationStrings.IsRunning, isRunning);
        animator.SetBool(AnimationStrings.isMoving, isMoving);
        animator.SetBool(AnimationStrings.isCrouching, isCrouching);
        animator.SetBool(AnimationStrings.isSliding, isSliding);
        animator.SetBool(AnimationStrings.isGrounded, isGrounded);
        animator.SetBool(AnimationStrings.isOnWall, isOnWall);
        animator.SetBool(AnimationStrings.isOnCeiling, isOnCeiling);


        if (startedJumping)
        {
            animator.SetTrigger(AnimationStrings.jump);
            startedJumping = false;
            return;
        }
    }

}
