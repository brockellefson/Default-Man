using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
    public CapsuleCollider2D standingHitbox;
    public CircleCollider2D crouchingHitbox;
    public LayerMask tileLayer;
    
    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.49f, 0.03f);
    [Header("WallCheck")]
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.49f, 0.03f);
        [Header("CrouchingWallCheck")]
    public Transform crouchingWallCheckPos;
    public Vector2 crouchingWallCheckSize = new Vector2(0.49f, 0.03f);
    [Header("CeilingCheck")]
    public Transform ceilingCheckPos;
    public Vector2 ceilingCheckSize = new Vector2(0.49f, 0.03f);
    [Header("CrouchingCeilingCheck")]
    public Transform crouchingceilingCheckPos;
    public Vector2 crouchingceilingCheckSize = new Vector2(0.49f, 0.03f);
    
    PlayerAnimator animator;
    private bool _isGrounded = true;
    private bool _isOnWall;
    private bool _isOnCeiling;
    private bool _isCrouching = false;
    public ContactFilter2D castFilter;

    public bool isGrounded 
    {
        get 
        {
            return _isGrounded;
        }
        private set
        {
            _isGrounded = value;
            animator.isGrounded = value;
        }
    }

    public bool isOnWall 
    {
        get 
        {
            return _isOnWall;
        }
        private set
        {
            _isOnWall = value;
            animator.isOnWall = value;
        }
    }

    public bool isOnCeiling 
    {
        get 
        {
            return _isOnCeiling;
        }
        private set
        {
            _isOnCeiling = value;
            animator.isOnCeiling = value;
        }
    }

    void Awake()
    {
        animator = GetComponent<PlayerAnimator>();
    }

    public void IsCrouchingOrRolling(bool crouching){
        _isCrouching = crouching;
        if(crouching){
            crouchingHitbox.enabled = true;
            standingHitbox.enabled = false;
        }
        else{
            standingHitbox.enabled = true;
            crouchingHitbox.enabled = false;
        }
    }

    void FixedUpdate()
    {
        isGrounded = TouchingGround();
        isOnWall = _isCrouching ? TouchingWallWhileCrouching() : TouchingWall();
        isOnCeiling = _isCrouching ? TouchingCeilingWhileCrouching() : TouchingCeiling();
    }

    private bool TouchingGround(){
        return Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, tileLayer);
    }
    private bool TouchingWall(){
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, tileLayer);
    }
    private bool TouchingWallWhileCrouching(){
        return Physics2D.OverlapBox(crouchingWallCheckPos.position, crouchingceilingCheckSize, 0, tileLayer);
    }
    private bool TouchingCeiling(){
        return Physics2D.OverlapBox(ceilingCheckPos.position, ceilingCheckSize, 0, tileLayer);
    }
    private bool TouchingCeilingWhileCrouching(){
        return Physics2D.OverlapBox(crouchingceilingCheckPos.position, crouchingceilingCheckSize, 0, tileLayer);
    }

    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.white;
        Gizmos.DrawCube(groundCheckPos.position, groundCheckSize);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(wallCheckPos.position, wallCheckSize);
        Gizmos.color = Color.black;
        Gizmos.DrawCube(ceilingCheckPos.position, ceilingCheckSize);
        Gizmos.DrawCube(crouchingWallCheckPos.position, crouchingWallCheckSize);
        Gizmos.DrawCube(crouchingceilingCheckPos.position, crouchingceilingCheckSize);
    }
}
