using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemySample : EnnemyHandler
{
    [Header("EnnemySample settings")]
    public float jumpTime;
    public float jumpForce;

    private float timeBeforeNextJump;

    private void Start()
    {
        playerGrapplingHandler = GameObject.FindWithTag("Player").GetComponent<PlayerGrapplingHandler>();
        playerAttackManager = GameObject.FindWithTag("Player").GetComponent<PlayerAttackManager>();
        rb = GetComponent<Rigidbody2D>();
        isStunned = false;
        isInvulnerable = false;
        currentHealth = maxHealth;
        timeBeforeNextJump = 3.0f;
    }

    private void Update()
    {
        if(timeBeforeNextJump > 0)
        {
            timeBeforeNextJump -= Time.deltaTime;
        }
        else
        {
            rb.velocity = Vector2.up * jumpForce;
            timeBeforeNextJump = jumpTime;
        }
    }
}
