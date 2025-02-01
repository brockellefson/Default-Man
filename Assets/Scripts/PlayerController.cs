using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{

    Vector2 moveInput;
    Rigidbody2D rb;
    Animator animator;
    TouchingDirections touchingDirections;
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpImpulse = 8f;

    public float CurrentMoveSpeed{ 
        get {
            if(IsMoving && !touchingDirections.isOnWall){
                if(IsRunning){
                    return runSpeed;
                }

                return walkSpeed;
            }

            return 0;
        }
    }
    public bool IsMoving { 
        get { return _isMoving;}
        private set {
            _isMoving = value;
            animator.SetBool(AnimationStrings.isMoving, value);
        }
    }

    public bool IsRunning { 
        get { return _isRunning;}
        private set {
            _isRunning = value;
            animator.SetBool(AnimationStrings.IsRunning, value);
        }
    }

    public bool isFacingRight { 
        get { return _isFacingRight;}
        private set {
            if(_isFacingRight != value){
                transform.localScale *= new Vector2(-1,1);
            }

            _isFacingRight = value;
        }
    }

    private bool _isMoving = false;
    private bool _isRunning = false;
    private bool _isFacingRight = true;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
    }

    void FixedUpdate(){
        rb.linearVelocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.linearVelocityY);
        animator.SetFloat(AnimationStrings.yVelocity, rb.linearVelocityY);
    }

    public void OnMove(InputAction.CallbackContext context) 
    {
        moveInput = context.ReadValue<Vector2>();
        IsMoving = moveInput != Vector2.zero;
        SetDirection(moveInput);
    }

    private void SetDirection(Vector2 moveInput)
    {
        if(moveInput.x > 0 && !isFacingRight)
        {
            isFacingRight = true;
        }
        else if(moveInput.x < 0 && isFacingRight)
        {
            isFacingRight = false;
        }
    }

    public void OnRun(InputAction.CallbackContext context) 
    {
        if(context.started)
        {
            IsRunning = true;
        }
        else if(context.canceled)
        {
            IsRunning = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context) 
    {
        if(context.started && touchingDirections.isGrounded)
        {
            animator.SetTrigger(AnimationStrings.jump);
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpImpulse);
        }
    }
}
