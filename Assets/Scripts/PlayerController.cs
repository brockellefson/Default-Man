using System;
using System.Data;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    public PlayerData Data;
    public Rigidbody2D RB;
    public PlayerAnimator animator;
    public PlayerCollider playerCollider;
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

        public bool isSprinting 
    { 
        get { return _isSprinting; }
        private set 
        {
            _isSprinting = value;
            animator.isSprinting = value;
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
                if(IsRunning && !IsSliding){

                }
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
    private bool _isSprinting = false;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool crouchBuffer = false;
    private bool standBuffer = false;

    void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        animator = GetComponent<PlayerAnimator>();
        playerCollider = GetComponent<PlayerCollider>();
    }

    private void Start()
    {
    }

    private void Update()
    {
        jumpBufferCounter -= Time.deltaTime;

        if(playerCollider.isGrounded){
            playerCollider.IsCrouchingOrRolling(IsCrouching || IsSliding);
            coyoteTimeCounter = Data.coyoteTime;
        }
        else{
            coyoteTimeCounter -= Time.deltaTime;
        }

        if(CanJump()){
            Jump();
        }

        if(standBuffer && !playerCollider.isOnCeiling){
            Stand();
            standBuffer = false;
        }

        if(CanRollOrCrouch()){
            CrouchOrRoll();
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
        IsMoving = moveInput.x != 0f;
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
            if(IsSliding){
                IsSliding = false;
            }

            ComeToHalt();
        }

    }

    public void ComeToHalt(){
        animator.comeToHalt = true;
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
        if(context.canceled && (IsCrouching || IsSliding)){
            standBuffer = true;
            crouchBuffer = false;
        }
        else if(context.started)
        {
            crouchBuffer = true;
        }  
    }

    private void SetDirection(Vector2 moveInput)
    {
        if(IsSliding){
            return;
        }

        if(moveInput.x > 0 && !isFacingRight)
        {
            if(IsRunning){

            }
            isFacingRight = true;
        }
        else if(moveInput.x < 0 && isFacingRight)
        {
            if(IsRunning){
                
            }
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
    if(IsSliding){
        return;
    }

    if(moveInput.x == 0 && !IsRunning){
            RB.linearVelocity = new Vector2(0, RB.linearVelocityY);
            return;
    }

    // Calculate the target speed based on input and max speed
    float targetSpeed = moveInput.x * Data.runMaxSpeed;

    // Lerp towards the target speed
    targetSpeed = Mathf.Lerp(RB.linearVelocityX, targetSpeed, lerpAmount);

    // Apply speed boost if running
    if (IsRunning)
    {
        targetSpeed *= Data.sprintAccel;
    }

    // Determine acceleration rate based on grounded or air state
    float accelRate;
    accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;

    // Apply jump hang modifiers if applicable
    if (IsJumping && Mathf.Abs(RB.linearVelocityY) < Data.jumpHangTimeThreshold)
    {
        accelRate *= Data.jumpHangAccelerationMult;
        targetSpeed *= Data.jumpHangMaxSpeedMult;
    }

    // Calculate speed difference and simulate force-based acceleration
    float speedDif = targetSpeed - RB.linearVelocityX;

    // Convert acceleration to match how AddForce works (Force = Mass * Acceleration)
    float accelerationForce = speedDif * accelRate;
    float movement = accelerationForce / RB.mass * Time.fixedDeltaTime;
    float finalSpeed = RB.linearVelocityX + movement;
    // Apply crouching modifier
    if (IsCrouching)
    {
        finalSpeed *= .7f;
    }

    // Update the Rigidbody's velocity
    RB.linearVelocity = new Vector2(finalSpeed, RB.linearVelocityY);

    if(Math.Abs(RB.linearVelocityX) >= Data.maxSprintSpeed * .99)
    {
        isSprinting = true;
    }
    else
    {
        isSprinting = false;
    }

    animator.SetXVelocity(RB.linearVelocityX);
}


    public void Jump()
    {
        animator.startedJumping = true;
        IsJumping = true;
        RB.linearVelocity = new Vector2(RB.linearVelocityX, Data.jumpForce);  
        jumpBufferCounter = 0;
    }

    public void CrouchOrRoll()
    {
        if(IsRunning){
                IsSliding = true;
            }
        else{
            IsCrouching = true;
        }

            crouchBuffer = false;
    }

    public void Stand(){
        IsCrouching = false;
        IsSliding = false;
    }

    public bool CanJump()
    {
        return coyoteTimeCounter > 0f && jumpBufferCounter > 0f && !IsSliding && !playerCollider.isOnCeiling;
    }

    public bool CanRollOrCrouch(){
        return crouchBuffer && playerCollider.isGrounded;
    }
}
