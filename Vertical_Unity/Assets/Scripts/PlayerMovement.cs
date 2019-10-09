using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool allowMouseControl;
    [Header("Base movement settings")]
    public float maxRunningSpeed;
    public float runningAcceleration;
    public float groundSlowing;
    [Header("Technical settings")]
    public Transform feetPos;
    public float groundCheckThickness = 0.1f;
    public float groundCheckWidth = 1.0f;
    public LayerMask walkableMask;

    private Vector2 targetVelocity;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetVelocity = Vector2.zero;
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
            targetVelocity.x = Input.GetAxis("LJoystickH") * maxRunningSpeed;
        }
        else if(targetVelocity.x != 0)
        {
            targetVelocity.x = 0;
        }
    }

    private void UpdateMovement()
    {
        if(IsOnGround())
        {
            float addedXVelocity = Mathf.Sign(targetVelocity.x - rb.velocity.x) * runningAcceleration * Time.fixedDeltaTime;
            if (Mathf.Sign(rb.velocity.x) != Mathf.Sign(rb.velocity.x + addedXVelocity) && targetVelocity.x == 0)
            {
                rb.velocity = new Vector2(0.0f, rb.velocity.y);
            }
            else if (rb.velocity.x < targetVelocity.x && targetVelocity.x > 0 || rb.velocity.x > targetVelocity.x && targetVelocity.x < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x + addedXVelocity, rb.velocity.y);
            }

            if(rb.velocity.x > targetVelocity.x && targetVelocity.x > 0 || rb.velocity.x < targetVelocity.x && targetVelocity.x < 0)
            {
                rb.velocity = new Vector2(targetVelocity.x, rb.velocity.y);
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
}
