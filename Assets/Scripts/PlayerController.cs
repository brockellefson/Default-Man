using System;
using System.Data;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    public PlayerData Data;
    public Rigidbody2D RB;
    public PlayerAnimator animator;
    public PlayerCollider collider;
    public Vector2 moveInput;

    public bool IsMoving 
    { 
        get { return _isMoving; }
        private set 
        {
            _isMoving = value;
            animator.isMoving = value;
        }
    }

    public bool IsRunning 
    { 
        get { return _isRunning;}
        private set 
        {
            _isRunning = value;
            animator.isRunning = value;
        }
    }

    public bool IsCrouching 
    { 
        get { return _isCrouching;}
        private set 
        {
            _isCrouching = value;
            animator.isCrouching = value;
        }
    }

    public bool IsSliding 
    { 
        get { return _isSliding;}
        private set 
        {
            _isSliding = value;
            animator.isSliding = value;
        }
    }

    public bool isFacingRight
    { 
        get { return _isFacingRight;}
        private set {
            if(_isFacingRight != value){
                transform.localScale *= new Vector2(-1,1);
            }

            _isFacingRight = value;
        }
    }

    public bool IsJumping = false;
    private bool _isMoving = false;
    private bool _isRunning = false;
    private bool _isCrouching = false;
    private bool _isSliding = false;
    private bool _isFacingRight = true;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        animator = GetComponent<PlayerAnimator>();
        collider = GetComponent<PlayerCollider>();
    }

    private void Start()
    {
    }

    private void Update()
    {
        jumpBufferCounter -= Time.deltaTime;

        if(collider.isGrounded){
            coyoteTimeCounter = Data.coyoteTime;
        }
        else{
            coyoteTimeCounter -= Time.deltaTime;
        }

        if(CanJump()){
            Jump();
        }

		if (IsJumping && RB.linearVelocityY < 0)
		{
			IsJumping = false;
		}
    }

    void FixedUpdate()
    {
        Run(1);
        SetGravity();
    }

    public void OnMove(InputAction.CallbackContext context) 
    {
        moveInput = context.ReadValue<Vector2>();
        IsMoving = moveInput != Vector2.zero;
        SetDirection(moveInput);
    }

    public void OnRun(InputAction.CallbackContext context) 
    {
        if(context.started && IsMoving && !IsCrouching)
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
        if(context.started)
        {
            jumpBufferCounter = Data.jumpInputBufferTime;
        }
        else if(context.canceled && RB.linearVelocityY > 0){
            RB.linearVelocity = new Vector2(RB.linearVelocityX, RB.linearVelocityY * .5f);
            coyoteTimeCounter = 0;
        }
    }

    public void OnCrouch(InputAction.CallbackContext context) 
    {
        if(context.started && collider.isGrounded)
        {
            if(IsRunning){
                IsSliding = true;
            }
            else{
                IsCrouching = true;
            }
        }
        else if(context.canceled){
            IsCrouching = false;
            IsSliding = false;
        }
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

    private void SetGravityScale(float scale)
	{
		RB.gravityScale = scale;
	}

    public void SetGravity()
    {
        if (RB.linearVelocityY < 0 && moveInput.y < 0)
        {
            SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
            RB.linearVelocity = new Vector2(RB.linearVelocityX, Mathf.Max(RB.linearVelocityY, -Data.maxFastFallSpeed));
        }
        else if(IsJumping && Mathf.Abs(RB.linearVelocityY) < Data.jumpHangTimeThreshold){
				SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
        }
        else if(RB.linearVelocityY < 0){
            SetGravityScale(Data.gravityScale * Data.fallGravityMult);
            RB.linearVelocity = new Vector2(RB.linearVelocityX, Mathf.Max(RB.linearVelocityY, -Data.maxFallSpeed));
        }
		else
		{
			SetGravityScale(Data.gravityScale);
		}

        animator.SetYVelocity(RB.linearVelocityY);
    }

    private void Run(float lerpAmount)
	{
		float targetSpeed = moveInput.x * Data.runMaxSpeed;
		targetSpeed = Mathf.Lerp(RB.linearVelocityX, targetSpeed, lerpAmount);
        if(IsRunning){
            targetSpeed *= (float) 1.3;
        }
        
        float accelRate;

		if (collider.isGrounded)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;

        if(IsJumping && Mathf.Abs(RB.linearVelocityY) < Data.jumpHangTimeThreshold){
			accelRate *= Data.jumpHangAccelerationMult;
			targetSpeed *= Data.jumpHangMaxSpeedMult;  
        }

		float speedDif = targetSpeed - RB.linearVelocityX;
		float movement = speedDif * accelRate;

        if(IsCrouching){
            movement = movement/2;
        }

		RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
	}

    public void Jump()
    {
        animator.startedJumping = true;
        IsJumping = true;
        RB.linearVelocity = new Vector2(RB.linearVelocityX, Data.jumpForce);  
        jumpBufferCounter = 0;
    }

    public bool CanJump()
    {
        return coyoteTimeCounter > 0f && jumpBufferCounter > 0f;
    }
}
