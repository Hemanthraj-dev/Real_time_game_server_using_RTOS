using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchingDirection : MonoBehaviour
{
    public ContactFilter2D castFilter;

    public float groundDistance = 0.05f;
    public float wallDistance = 0.05f;
    public float ceilingDistance = 0.05f;

    // Small grace period to prevent ground detection flickering
    public float groundGraceTime = 0.08f;

    private float groundLostTimer = 0f;

    CapsuleCollider2D touchingCol;
    Animator animator;

    RaycastHit2D[] groundhits = new RaycastHit2D[5];
    RaycastHit2D[] wallHits = new RaycastHit2D[5];
    RaycastHit2D[] ceilingHits = new RaycastHit2D[5];

    [SerializeField]
    private bool _isGrounded;

    public bool IsGrounded
    {
        get
        {
            return _isGrounded;
        }
        private set
        {
            _isGrounded = value;
            animator.SetBool(AnimationStrings.isGrounded, value);
        }
    }

    [SerializeField]
    private bool _isOnWall;

    public bool IsOnWall
    {
        get
        {
            return _isOnWall;
        }
        private set
        {
            _isOnWall = value;
            animator.SetBool(AnimationStrings.isOnWall, value);
        }
    }

    [SerializeField]
    private bool _isOnCeiling;

    private Vector2 wallCheckDirection =>
        gameObject.transform.localScale.x > 0
        ? Vector2.right
        : Vector2.left;

    public bool IsOnCeiling
    {
        get
        {
            return _isOnCeiling;
        }
        private set
        {
            _isOnCeiling = value;
            animator.SetBool(AnimationStrings.isOnCeiling, value);
        }
    }

    private void Awake()
    {
        touchingCol = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        // Ground check
        bool groundDetected =
            touchingCol.Cast(
                Vector2.down,
                castFilter,
                groundhits,
                groundDistance
            ) > 0;

        if (groundDetected)
        {
            // Ground detected
            groundLostTimer = 0f;
            IsGrounded = true;
        }
        else
        {
            // Ground temporarily not detected
            groundLostTimer += Time.fixedDeltaTime;

            if (groundLostTimer >= groundGraceTime)
            {
                IsGrounded = false;
            }
        }

        // Wall check
        IsOnWall =
            touchingCol.Cast(
                wallCheckDirection,
                castFilter,
                wallHits,
                wallDistance
            ) > 0;

        // Ceiling check
        IsOnCeiling =
            touchingCol.Cast(
                Vector2.up,
                castFilter,
                ceilingHits,
                ceilingDistance
            ) > 0;
    }
}