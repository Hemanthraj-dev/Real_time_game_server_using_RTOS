using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirection), typeof(Damageable))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float runSpeed = 20f;
    public float airWalkSpeed = 3f;
    public float jumpImpulse = 10f;
    public float longJumpImpulse = 15f;

   Vector2 moveInput;

    private bool jumpRequested;
    private bool attackRequested;

    public Vector2 MoveInput => moveInput;

    TouchingDirection touchingDirection;
    Damageable damageable;

    public float CurrentMoveSpeed
    {
        get
        {
            if (CanMove)
            {
                if (IsMoving && !touchingDirection.IsOnWall)
                {
                    if (touchingDirection.IsGrounded)
                    {
                        if (IsRunning)
                        {
                            return runSpeed;
                        }
                        else
                        {
                            return walkSpeed;
                        }
                    }
                    else
                    {
                        // Air Move
                        return airWalkSpeed;
                    }
                }
                else
                {
                    // Idle speed is 0
                    return 0;
                }
            }
            else
            {
                // Movement Locked
                return 0;
            }
        }
    }

    [SerializeField]
    private bool _isMoving = false;

    public bool IsMoving
    {
        get
        {
            return _isMoving;
        }
        private set
        {
            _isMoving = value;
            animator.SetBool(AnimationStrings.isMoving, value);
        }
    }

    [SerializeField]
    private bool isRunning = false;

    public bool IsRunning
    {
        get
        {
            return isRunning;
        }
        set
        {
            isRunning = value;
            animator.SetBool(AnimationStrings.isRunning, value);
        }
    }

    public bool _isFacingRight = true;

    public bool IsFacingRight
    {
        get
        {
            return _isFacingRight;
        }
        private set
        {
            if (_isFacingRight != value)
            {
                transform.localScale *= new Vector2(-1, 1);
            }

            _isFacingRight = value;
        }
    }

    public bool CanMove
    {
        get
        {
            return animator.GetBool(AnimationStrings.canMove);
        }
    }

    public bool LockVelocity 
    {
        get
        {
            return animator.GetBool(AnimationStrings.lockVelocity);
        }
    }

    Rigidbody2D rb;
    Animator animator;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirection = GetComponent<TouchingDirection>();
        damageable = GetComponent<Damageable>();
    }

   private void FixedUpdate()
{
    // Server is authoritative for position now — do NOT write rb.linearVelocity
    // here anymore, it only fights the STATE packets from NetworkManager and
    // causes jitter. Keep this purely for animation feedback.
    animator.SetFloat(
        AnimationStrings.yVelocity,
        rb.linearVelocity.y
    );
}

public void OnJump(InputAction.CallbackContext context)
{
    if (context.started && touchingDirection.IsGrounded)
    {
        jumpRequested = true;

        Debug.Log("Jump Input Detected");

        animator.SetTrigger(AnimationStrings.jump);

        
    }
}

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (damageable.IsAlive)
        {
            IsMoving = moveInput != Vector2.zero;
            SetFacingDirection(moveInput);
        }
        else
        {
            PlayerInput input = GetComponent<PlayerInput>();
            input.actions.Disable();
        }
    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if (moveInput.x > 0 && !IsFacingRight)
        {
            // Face right
            IsFacingRight = true;
        }
        else if (moveInput.x < 0 && IsFacingRight)
        {
            // Face left
            IsFacingRight = false;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsRunning = true;
        }
        else if (context.canceled)
        {
            IsRunning = false;
        }
    }



   public void OnAttack(InputAction.CallbackContext context)
{
    if (context.started)
    {
        attackRequested = true;

        animator.SetTrigger(AnimationStrings.attackTrigger);
    }
}
    public void OnHit(int damage, Vector2 knockback)
    {
        rb.linearVelocity = new Vector2(
            knockback.x,
            rb.linearVelocity.y + knockback.y
        );
    }


    public bool ConsumeJumpRequest()
{
    bool requested = jumpRequested;
    jumpRequested = false;
    return requested;
}

public bool ConsumeAttackRequest()
{
    bool requested = attackRequested;
    attackRequested = false;
    return requested;
}


}