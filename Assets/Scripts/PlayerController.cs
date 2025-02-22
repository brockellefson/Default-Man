using System;
using System.Data;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.ShaderGraph;
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
    public Vector2 lastMoveInput;
    
    public bool IsWalking 
    { 
        get { return _isWalking; }
        private set 
        {
            _isWalking = value;
            animator.isWalking = value;
        }
    }

    public bool IsSprinting 
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

    public bool IsWallRunning 
    { 
        get { return _isWallRunning;}
        private set 
        {
            _isWallRunning = value;
            animator.isWallRunning = value;
        }
    }

    public bool TurnWhileSprinting 
    { 
        get { return _turnWhileSprinting;}
        private set 
        {
            _turnWhileSprinting = value;
        }
    }

    public bool isFacingRight
    { 
        get { return _isFacingRight;}
        private set {
            if(_isFacingRight != value){
                if(TurnWhileSprinting){
                    RB.linearVelocityX = sprintSpeedBeforeHalt * -1;
                    TurnWhileSprinting = false;
                    IsSprinting = true;
                    IsWalking = false;
                } 

                transform.localScale *= new Vector2(-1,1);
            }

            _isFacingRight = value;
        }
    }
	[Header("States")]
    [SerializeField]
    public bool IsJumping = false;
    public bool Halt = false;
    [SerializeField]
    private bool _isWalking = false;
    [SerializeField]
    private bool _isRunning = false;
    [SerializeField]
    private bool _isWallRunning = false;
    [SerializeField]
    private bool _isCrouching = false;
    [SerializeField]
    private bool _isSliding = false;
    [SerializeField]
    private bool _isSprinting = false;
    [SerializeField]
    private bool _turnWhileSprinting = false;
    private bool _isFacingRight = true;


    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool crouchBuffer = false;
    private bool standBuffer = false;
    private float sprintSpeedBeforeHalt;
    private float sprintDirectionBeforeHalt;
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

		moveInput.x = Input.GetAxisRaw("Horizontal");

        
        if(CanWallRun()){
            IsWallRunning = true;
        }
        else{
            IsWallRunning = false;
        }

        DetermineMovementState();

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

    public void DetermineMovementState()
    {
        if(Halt){
            moveInput.x = 0f;

            if(Math.Abs(RB.linearVelocityX) <= .5f){
                Halt = false;
            }
        }
        else if(IsWallRunning){
            OnWallRun();
        }
        else if(IsRunning){
            if(isFacingRight){
		        moveInput.x = 1;
            }
            else{
                moveInput.x = -1;
            }
        }
        else if(IsSprinting){
            SetDirectionWhileSprinting();
        }
        else{
            OnWalk();
        }
    }

    void FixedUpdate()
    {
        Run(1);
        SetGravity();
    }

    public void OnWalk() 
    {
        IsWalking = moveInput.x != 0f;
        SetDirection(moveInput);
    }

    public void OnRun(InputAction.CallbackContext context) 
    {
        if(context.started && !IsCrouching)
        {
            IsRunning = true;
            IsWalking = false;
        }
        else if(context.canceled)
        {
            IsRunning = false;

            if(IsSliding){
                IsSliding = false;
            }

            if(IsSprinting){
                ComeToHalt();
            }
        }

    }

    public void ComeToHalt(){
        IsWalking = false;
        IsSprinting = false;
        IsRunning = false;
        Halt = true;
    }

    public void OnWallRun(){
   
        float targetSpeed = Data.wallRunMaxSpeed;

        targetSpeed = Mathf.Lerp(RB.linearVelocityY, targetSpeed, 1);

        float accelRate;
        accelRate = Data.wallRunAccelAmount;

        float speedDif = targetSpeed - RB.linearVelocityY;

        float accelerationForce = speedDif * accelRate;
        float movement = accelerationForce / RB.mass * Time.fixedDeltaTime;
        float finalSpeed = RB.linearVelocityY + movement;



        // Update the Rigidbody's velocity
        RB.linearVelocity = new Vector2(RB.linearVelocityX, finalSpeed);
        SetGravityScale(0);
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

        if(TurnWhileSprinting){
            moveInput.x = sprintDirectionBeforeHalt;
        }

        if(moveInput.x > 0 && !isFacingRight)
        {           
            isFacingRight = true;
        }
        else if(moveInput.x < 0 && isFacingRight)
        {
            isFacingRight = false;   
        }
    }

    private void SetDirectionWhileSprinting()
    {
        sprintSpeedBeforeHalt = RB.linearVelocityX;

        if(moveInput.x > 0 && !isFacingRight)
        {            
            sprintDirectionBeforeHalt = moveInput.x;
            TurnWhileSprinting = true;
            ComeToHalt();
        }
        else if(moveInput.x < 0 && isFacingRight)
        {
            sprintDirectionBeforeHalt = moveInput.x;
            TurnWhileSprinting = true;
            ComeToHalt();
        }

        if(isFacingRight){
		    moveInput.x = 1;
        }
        else{
            moveInput.x = -1;
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

    if(moveInput.x == 0 && !Halt){
            RB.linearVelocity = new Vector2(0, RB.linearVelocityY);
            return;
    }

    // Calculate the target speed based on input and max speed
    float targetSpeed = moveInput.x * Data.runMaxSpeed;

    // Lerp towards the target speed
    targetSpeed = Mathf.Lerp(RB.linearVelocityX, targetSpeed, lerpAmount);

    // Apply speed boost if running
    if (IsRunning || IsSprinting)
    {
        targetSpeed *= Data.sprintAccel;
    }

    // Determine acceleration rate based on grounded or air state
    float accelRate;
    accelRate = (Mathf.Abs(targetSpeed) > 0.01f) && !Halt ? Data.runAccelAmount : Data.runDeccelAmount;

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
        finalSpeed *= .9f;
    }

    // Update the Rigidbody's velocity
    RB.linearVelocity = new Vector2(finalSpeed, RB.linearVelocityY);

    if(Math.Abs(RB.linearVelocityX) >= Data.maxSprintSpeed * .99)
    {
        IsSprinting = true;
        IsRunning = false;
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
        if(IsRunning || IsSprinting){
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

    public bool CanWallRun(){
        return (IsRunning || IsSprinting) && playerCollider.isOnWall && !playerCollider.isGrounded;
    }
}
