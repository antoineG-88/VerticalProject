using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider_Bot : EnemyHandler
{
    [Header("OverBot settings")]
    public float jumpSpeed;
    [Header("JumpShot")]
    public int jumpShotDamage;
    public float jumpShotKnockbackForce;
    public float jumpShotStunTime;
    public float detectionRange;
    public float jumpShotDamageRange;
    public float jumpShotCooldown;
    public float jumpShotDelay;
    public float jumpShotAngleRange;
    public surfaceDirection startSurfaceDirection;
    public Transform spriteTransform;
    [Header("Debug settings")]
    public GameObject particleDebugPrefab;


    private float timeBeforeJumpShot;
    private float jumpShotCooldownRemaining;
    private Vector2 jumpDirection;
    private Vector2 spiderBotPosition;
    private bool playerAtRange;
    private bool onSurface;
    private surfaceDirection currentSurfaceDirection;
    private float jumpShotDelayRemaining;
    private bool isTouchingWall;
    private Vector2 collisionPoint;
    private bool isJumping;
    private float jumpCollisionDelay;

    public enum surfaceDirection{ up, down, right, left, none};

    void Start()
    {
        HandlerStart();

        timeBeforeJumpShot = 0;
        jumpShotCooldownRemaining = 0;
        playerAtRange = false;
        onSurface = true;
        isTouchingWall = false;
        currentSurfaceDirection = startSurfaceDirection;
    }


    void Update()
    {
        HandlerUpdate();
        UpdateMovement();
    }

    private void FixedUpdate()
    {
        HandlerFixedUpdate();

        isTouchingWall = false;
    }

    public override bool TestCounter()
    {
        // counter avec dégats
        return isJumping;
    }

    public override void UpdateMovement()
    {
        if (!Is(Effect.Stun) && !Is(Effect.NoControl) && !Is(Effect.NoGravity))
        {
            if (onSurface && jumpShotCooldownRemaining <= 0)
            {
                jumpShotDelayRemaining -= Time.deltaTime;
                if(jumpShotDelayRemaining <= 0)
                {
                    float jumpCenterAngle = GetAngleFromSurfaceDirection(currentSurfaceDirection);
                    Debug.Log(jumpCenterAngle);
                    float jumpAngle = Random.Range(jumpCenterAngle - (jumpShotAngleRange / 2), jumpCenterAngle + (jumpShotAngleRange / 2));
                    Vector2 jumpDirection = new Vector2(Mathf.Cos(jumpAngle * Mathf.Deg2Rad), Mathf.Sin(jumpAngle * Mathf.Deg2Rad));
                    jumpDirection.Normalize();
                    Debug.DrawRay(transform.position, jumpDirection * 5, Color.blue, 1.0f);
                    StartCoroutine(JumpShot(jumpDirection));
                }
            }
            else
            {
                jumpShotDelayRemaining = jumpShotDelay;
            }
        }
        else
        {
            jumpShotDelayRemaining = jumpShotDelay;
        }

        if(jumpShotCooldownRemaining > 0)
        {
            jumpShotCooldownRemaining -= Time.deltaTime;
        }

        if(jumpCollisionDelay > 0)
        {
            jumpCollisionDelay -= Time.deltaTime;
        }

        if(onSurface)
        {
            Propel(Vector2.zero, true, true);
        }

        if (PlayerInSight())
        {
            playerAtRange = true;
        }
        else
        {
            playerAtRange = false;
        }
    }
    private IEnumerator JumpShot(Vector2 jumpShotDirection)
    {
        jumpShotCooldownRemaining = jumpShotCooldown;
        isJumping = true;
        onSurface = false;
        jumpCollisionDelay = 0.2f;
        while(!isTouchingWall)
        {
            Propel(jumpShotDirection * jumpSpeed, true, true);

            if(Physics2D.OverlapCircle(transform.position, jumpShotDamageRange, LayerMask.GetMask("Player")))
            {
                Vector2 playerDirection = GameData.playerMovement.transform.position - transform.position;
                playerDirection.Normalize();
                GameData.playerManager.TakeDamage(jumpShotDamage, playerDirection * jumpShotKnockbackForce, jumpShotStunTime);
            }
            yield return new WaitForFixedUpdate();
        }
        isJumping = false;
        onSurface = true;
        Propel(Vector2.zero, true, true);

        Vector2 vectorSurfaceDirection = (Vector2)transform.position - collisionPoint;
        currentSurfaceDirection = GetDirectionFromVector(vectorSurfaceDirection);
        spriteTransform.rotation = Quaternion.Euler(0.0f, 0.0f, GetAngleFromSurfaceDirection(currentSurfaceDirection) - 90);
    }

    private float GetAngleFromSurfaceDirection(surfaceDirection direction)
    {
        float angle = 0;
        switch(direction)
        {
            case surfaceDirection.up:
                angle = 90;
                break;

            case surfaceDirection.down:
                angle = 270;
                break;

            case surfaceDirection.right:
                angle = 0;
                break;

            case surfaceDirection.left:
                angle = 180;
                break;
        }

        return angle;
    }

    private surfaceDirection GetDirectionFromVector(Vector2 direction)
    {
        float minCollisionAngleDifference = 360;
        int i = 0;
        surfaceDirection lastSurfaceDirection = surfaceDirection.none;
        while (i < 4 && minCollisionAngleDifference >= 45)
        {
            Vector2 originDirection = Vector2.zero;
            switch(i)
            {
                case 0:
                    originDirection = Vector2.up;
                    lastSurfaceDirection = surfaceDirection.up;
                    break;

                case 1:
                    originDirection = Vector2.down;
                    lastSurfaceDirection = surfaceDirection.down;
                    break;

                case 2:
                    originDirection = Vector2.right;
                    lastSurfaceDirection = surfaceDirection.right;
                    break;

                case 3:
                    originDirection = Vector2.left;
                    lastSurfaceDirection = surfaceDirection.left;
                    break;
            }

            float collisionAngle = Vector2.Angle(originDirection, direction);
            if(minCollisionAngleDifference > collisionAngle)
            {
                minCollisionAngleDifference = collisionAngle;
            }
            i++;
        }

        return lastSurfaceDirection;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Ground") && jumpCollisionDelay <= 0)
        {
            isTouchingWall = true;
            collisionPoint = collision.contacts[0].point;
        }
    }
}
