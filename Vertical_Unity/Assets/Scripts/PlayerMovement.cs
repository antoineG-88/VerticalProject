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
            targetVelocity.x = Input.GetAxis("LJoystickH");
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
            float addedXVelocity = Mathf.Sign(targetVelocity.x * maxRunningSpeed - rb.velocity.x) * runningAcceleration * Time.fixedDeltaTime; //the added horizontal velocity needed to get closer to the target velocity
            if (targetVelocity.x == 0 && Mathf.Sign(rb.velocity.x + addedXVelocity) != Mathf.Sign(rb.velocity.x))
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
                Debug.Log("tozero");
            }
            else if(targetVelocity.x != 0)
            {
                rb.velocity = new Vector2(rb.velocity.x + addedXVelocity, rb.velocity.y);
            }

            if (rb.velocity.x > maxRunningSpeed)
            {
                new Vector2(maxRunningSpeed, rb.velocity.y);
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
