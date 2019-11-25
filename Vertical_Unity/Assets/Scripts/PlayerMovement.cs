using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Base movement settings")]
    public float maxTargetSpeed;
    public float runningAcceleration;
    public float groundSlowing;
    public float airAcceleration;
    public float airSlowing;
    public float airDragForce;
    public float jumpForce;
    public float baseGravityForce;
    [Range(0, 100)] public float jumpXVelocityGain;
    [Header("Dash settings")]
    public float dashDistance;
    public float dashSpeed;
    public float dashStopDistance;
    public bool dashBreakRope;
    public float dashCooldown;
    public bool invulnerableDash;
    [Range(0, 100)] public float dashVelocityKept;
    [Header("Technical settings")]
    public Transform feetPos;
    public float groundCheckThickness = 0.1f;
    public float groundCheckWidth = 1.0f;
    public LayerMask groundMask;

    [HideInInspector] public PlatformHandler currentPlayerPlatform;
    [HideInInspector] public bool inControl;
    [HideInInspector] public bool isAffectedbyGravity;
    [HideInInspector] public float gravityForce;
    [HideInInspector] public Vector2 targetVelocity;
    private Rigidbody2D rb;
    private float addedXVelocity;
    private bool jumpFlag;
    private float timeBeforeControl;
    [HideInInspector] public bool isDashing;
    private float dashCooldownRemaining;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetVelocity = Vector2.zero;
        jumpFlag = false;
        inControl = true;
        isAffectedbyGravity = true;
        isDashing = false;
        timeBeforeControl = 0;
        gravityForce = baseGravityForce;
    }

    private void Update()
    {
        UpdateInput();
    }

    private void FixedUpdate()
    {
        UpdateMovement();

        if(timeBeforeControl > 0)
        {
            timeBeforeControl -= Time.fixedDeltaTime;
        }
    }

    private void UpdateInput()
    {
        if(GameData.gameController.input.leftJoystickHorizontal != 0)
        {
            targetVelocity.x = GameData.gameController.input.leftJoystickHorizontal * maxTargetSpeed;
        }
        else if(targetVelocity.x != 0)
        {
            targetVelocity.x = 0;
        }

        if (GameData.gameController.input.jump && !isDashing && dashCooldownRemaining <= 0/*&& IsOnGround()*/)
        {
            //jumpFlag = true;
            StartCoroutine(Dash());
        }

        if(dashCooldownRemaining > 0)
        {
            dashCooldownRemaining -= Time.deltaTime;
        }
    }

    private void UpdateMovement()
    {
        if(inControl && timeBeforeControl <= 0 && !isDashing)
        {
            if (targetVelocity.x != rb.velocity.x)
            {
                float xDirection = Mathf.Sign(targetVelocity.x - rb.velocity.x);
                if (targetVelocity.x > 0 && rb.velocity.x < targetVelocity.x || targetVelocity.x < 0 && rb.velocity.x > targetVelocity.x)
                {
                    if (IsOnGround())
                    {
                        addedXVelocity = xDirection * runningAcceleration * Time.fixedDeltaTime;
                    }
                    else
                    {
                        addedXVelocity = xDirection * airAcceleration * Time.fixedDeltaTime;
                    }
                }
                else
                {
                    if (IsOnGround())
                    {
                        addedXVelocity = xDirection * groundSlowing * Time.fixedDeltaTime;
                    }
                    else
                    {
                        addedXVelocity = xDirection * airSlowing * Time.fixedDeltaTime;
                    }
                }

                if (targetVelocity.x > rb.velocity.x && targetVelocity.x < rb.velocity.x + addedXVelocity || targetVelocity.x < rb.velocity.x && targetVelocity.x > rb.velocity.x + addedXVelocity)
                {
                    rb.velocity = new Vector2(targetVelocity.x, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(rb.velocity.x + addedXVelocity, rb.velocity.y);
                }
            }

            if (jumpFlag)
            {
                Propel(new Vector2(targetVelocity.x * jumpXVelocityGain / 100, jumpForce), false, true);
                jumpFlag = false;
            }
        }

        if(isAffectedbyGravity)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y - gravityForce * Time.fixedDeltaTime);
        }

        if(!IsOnGround() && !GameData.playerGrapplingHandler.isTracting)
        {
            rb.velocity -= rb.velocity.normalized * airDragForce * Time.fixedDeltaTime;
        }
    }

    public bool IsOnGround()
    {
        bool isGrounded = false;
        if (Physics2D.OverlapBox(feetPos.position, new Vector2(groundCheckWidth, groundCheckThickness), 0.0f, groundMask) != null)
        {
            isGrounded = true;
        }

        return isGrounded;
    }

    public void Propel(Vector2 directedForce, bool resetHorizontalVelocity, bool resetVerticalVelocity)
    {
        Vector2 newVelocity = Vector2.zero;

        if(resetVerticalVelocity)
        {
            newVelocity.y = directedForce.y;
        }
        else
        {
            newVelocity.y = rb.velocity.y + directedForce.y;
        }

        if (resetHorizontalVelocity)
        {
            newVelocity.x = directedForce.x;
        }
        else
        {
            newVelocity.x = rb.velocity.x + directedForce.x;
        }

        rb.velocity = new Vector2(newVelocity.x, newVelocity.y);
    }

    public void DisableControl(float noControlTime, bool isAdded)
    {
        if(isAdded)
        {
            timeBeforeControl += noControlTime;
        }
        else
        {
            timeBeforeControl = noControlTime;
        }
    }

    private IEnumerator Dash()
    {
        if(dashBreakRope)
        {
            GameData.playerGrapplingHandler.ReleaseHook();
        }
        dashCooldownRemaining = (dashDistance / dashSpeed) + 1;
        isDashing = true;
        isAffectedbyGravity = false;
        Vector2 dashDirection = new Vector2(GameData.gameController.input.leftJoystickHorizontal, GameData.gameController.input.leftJoystickVertical).normalized;
        if(dashDirection == Vector2.zero)
        {
            dashDirection = Vector2.up;
        }

        Propel(dashDirection * dashSpeed, true, true);
        float timer = dashDistance / dashSpeed;
        Vector2 origin = transform.position;

        while (!GameData.playerManager.isStunned && timer > 0 && !Physics2D.Raycast(transform.position, dashDirection, dashStopDistance, LayerMask.GetMask("Ground")))
        {
            timer -= Time.fixedDeltaTime;
            if(invulnerableDash)
            {
                GameData.playerManager.isDodging = true;
            }

            yield return new WaitForFixedUpdate();
        }

        if(timer <= 0 && !Physics2D.Raycast(origin, dashDirection, dashDistance, LayerMask.GetMask("Ground")))
        {
            transform.position = origin + dashDirection * dashDistance;
        }

        GameData.playerManager.isDodging = false;
        isAffectedbyGravity = true;
        Propel(dashDirection * dashSpeed * dashVelocityKept / 100, true, true);
        isDashing = false;
        dashCooldownRemaining = dashCooldown;
    }
}
