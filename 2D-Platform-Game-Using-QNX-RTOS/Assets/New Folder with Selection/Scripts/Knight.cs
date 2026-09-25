using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirection))]
public class Knight : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float walkStopRate = 0.6f;
    public DetectionZone attackZone;
    public DetectionZone cliffDetectionZone;

    Rigidbody2D rb;
    TouchingDirection touchingDirection;
    Damageable damageable;
    Animator animator;

    public enum WalkableDirection
    {
        Right,
        Left
    }

    [SerializeField]
    private WalkableDirection _walkDirection = WalkableDirection.Right;
    private Vector2 walkDirectionVector = Vector2.right;

    private void Start()
{
    if (_walkDirection == WalkableDirection.Right)
    {
        walkDirectionVector = Vector2.right;
    }
    else
    {
        walkDirectionVector = Vector2.left;
    }
}

    public WalkableDirection WalkDirection
    {
        get { return _walkDirection; }
        set
        {
            if (_walkDirection != value)
            {
                gameObject.transform.localScale = new Vector2(
                    gameObject.transform.localScale.x * -1,
                    gameObject.transform.localScale.y
                );

                if (value == WalkableDirection.Right)
                {
                    walkDirectionVector = Vector2.right;
                }
                else if (value == WalkableDirection.Left)
                {
                    walkDirectionVector = Vector2.left;
                }
            }

            _walkDirection = value;
        }
    }

    public bool _hasTarget = false;

    public bool HasTarget
    {
        get { return _hasTarget; }
        private set
        {
            _hasTarget = value;
            animator.SetBool(AnimationStrings.hasTarget, value);
        }
    }

    public bool CanMove
    {
        get
        {
            return animator.GetBool(AnimationStrings.canMove);
        }
    }

    public float AttackCooldown { get
        {
            return animator.GetFloat(AnimationStrings.attackCooldown);
        } private set
        {
            animator.SetFloat(AnimationStrings.attackCooldown, Mathf.Max(value, 0));
        } }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirection = GetComponent<TouchingDirection>();
        animator = GetComponent<Animator>();
        damageable = GetComponent<Damageable>();
    }

    private void Update()
    {
        HasTarget = attackZone.detectedColliders.Count > 0;

        if(AttackCooldown > 0)
        {
            AttackCooldown -= Time.deltaTime;
        }
        
    }

    private void FixedUpdate()
{
    // Turn around if we reach a wall
    if (touchingDirection.IsGrounded && touchingDirection.IsOnWall)
    {
        FlipDirection();
    }

    // Turn around if there is no ground ahead
    if (touchingDirection.IsGrounded &&
        cliffDetectionZone.detectedColliders.Count == 0)
    {
        FlipDirection();
    }

    if (CanMove)
    {
        rb.linearVelocity = new Vector2(
            walkSpeed * walkDirectionVector.x,
            rb.linearVelocity.y
        );
    }
    else
    {
        rb.linearVelocity = new Vector2(
            Mathf.Lerp(rb.linearVelocity.x, 0, walkStopRate),
            rb.linearVelocity.y
        );
    }
}

    private void FlipDirection()
    {
        if (WalkDirection == WalkableDirection.Right)
        {
            WalkDirection = WalkableDirection.Left;
        }
        else if (WalkDirection == WalkableDirection.Left)
        {
            WalkDirection = WalkableDirection.Right;
        }
        else
        {
            Debug.LogError("Current Walkable Direction is not set to legal values of Right or Left");
        }
    }

    public void OnHit(int damage, Vector2 knockback)
    {
        rb.linearVelocity = new Vector2(
            knockback.x,
            rb.linearVelocity.y + knockback.y
        );
    }
}