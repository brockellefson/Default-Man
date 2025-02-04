using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
    Collider2D currentCollider;
    PlayerAnimator animator;
    RaycastHit2D[] groundHits = new RaycastHit2D[5];
    RaycastHit2D[] wallHits = new RaycastHit2D[5];
    RaycastHit2D[] ceilingHits = new RaycastHit2D[5];
    public CapsuleCollider2D standingCollider;
    public CircleCollider2D crouchingCollider;
    private bool _isGrounded = true;
    private bool _isOnWall;
    private bool _isOnCeiling;

    private Vector2 wallCheckDirection => gameObject.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
    public ContactFilter2D castFilter;
    public float groundDistance = 0.05f;
    public float ceilingDistance = 0.15f;
    public float wallDistance = 0.05f;
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
        currentCollider = standingCollider;
        animator = GetComponent<PlayerAnimator>();
    }

    public void IsCrouchingOrRolling(bool crouching){
        if(crouching){
            crouchingCollider.enabled = true;
            currentCollider = crouchingCollider;
            standingCollider.enabled = false;
        }
        else{
            standingCollider.enabled = true;
            currentCollider = standingCollider;
            crouchingCollider.enabled = false;
        }
    }

    private void Update(){

    }

    void FixedUpdate()
    {
        isGrounded = currentCollider.Cast(Vector2.down, castFilter, groundHits, groundDistance) > 0;
        isOnWall = false;//currentCollider.Cast(wallCheckDirection, castFilter, wallHits, wallDistance) > 0;
        isOnCeiling = false;//currentCollider.Cast(Vector2.up, castFilter, ceilingHits, ceilingDistance) > 0;
    }

    // [Header("GroundCheck")]
    // public Transform groundCheckPos;
    // public Vector2 groundCheckSize = new Vector2(0.49f, 0.03f);
    // public LayerMask groundLayer;
    // [Header("WallCheck")]
    // public Transform wallCheckPos;
    // public Vector2 wallCheckSize = new Vector2(0.49f, 0.03f);
    // public LayerMask wallLayer;
    // [Header("CeilingCheck")]
    // public Transform ceilingCheckPos;
    // public Vector2 ceilingCheckSize = new Vector2(0.49f, 0.03f);
    // public LayerMask ceilingLayer;

    // void FixedUpdate()
    // {
    //     isGrounded = TouchingGround();
    //     isOnWall = TouchingWall();
    //     isOnCeiling = TouchingCeiling();
    // }

    // private bool TouchingGround(){
    //     return Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, layer);
    // }
    // private bool TouchingWall(){
    //     return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, layer);
    // }
    // private bool TouchingCeiling(){
    //     return Physics2D.OverlapBox(ceilingCheckPos.position, ceilingCheckSize, 0, layer);
    // }

    // private void OnDrawGizmosSelected(){
    //     Gizmos.color = Color.white;
    //     Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireCube(ceilingCheckPos.position, ceilingCheckSize);
    // }
}
