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
    public float ceilingDistance = 0f;
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
        currentCollider = crouchingCollider;
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
        isOnWall = currentCollider.Cast(wallCheckDirection, castFilter, wallHits, wallDistance) > 0;
        isOnCeiling = currentCollider.Cast(Vector2.up, castFilter, ceilingHits, ceilingDistance) > 0;
    }
}
