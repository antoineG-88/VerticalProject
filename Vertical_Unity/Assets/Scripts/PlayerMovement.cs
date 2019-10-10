using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool allowMouseControl;
    [Header("Base movement settings")]
    public float maxTargetSpeed;
    public float runningAcceleration;
    public float groundSlowing;
    public float airAcceleration;
    public float airSlowing;
    public float jumpForce;
    [Range(0, 100)] public float jumpXVelocityGain;
    [Header("Technical settings")]
    public Transform feetPos;
    public float groundCheckThickness = 0.1f;
    public float groundCheckWidth = 1.0f;
    public LayerMask walkableMask;

    [HideInInspector] public bool inControl;
    private Vector2 targetVelocity;
    private Rigidbody2D rb;
    private float addedXVelocity;
    private bool jumpFlag;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetVelocity = Vector2.zero;
        jumpFlag = false;
        inControl = true;
    }


    private void Update()
    {
        UpdateInput();
    }

    private void FixedUpdate()
    {
        UpdateMovement();
    }

    private void UpdateInput()
    {
        if(Input.GetAxis("LJoystickH") != 0)
        {
            targetVelocity.x = Input.GetAxis("LJoystickH") * maxTargetSpeed;
        }
        else if(targetVelocity.x != 0)
        {
            targetVelocity.x = 0;
        }

        if (Input.GetButton("AButton") && IsOnGround())
        {
            jumpFlag = true;
        }
    }

    private void UpdateMovement()
    {
        if(inControl)
        {
            if (targetVelocity.x != rb.velocity.x)
            {
                float xDirection = Mathf.Sign(targetVelocity.x - rb.velocity.x);
                if (rb.velocity.x > 0 && rb.velocity.x < targetVelocity.x || rb.velocity.x < 0 && rb.velocity.x > targetVelocity.x)
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
    }

    public bool IsOnGround()
    {
        bool isGrounded = false;
        if (Physics2D.OverlapBox(feetPos.position, new Vector2(groundCheckWidth, groundCheckThickness), 0.0f, walkableMask) != null)
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
}
