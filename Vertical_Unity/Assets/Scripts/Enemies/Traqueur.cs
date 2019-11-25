using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class Traqueur : EnemyHandler
{
    [Header("Traqueur settings")]
    public float jumpForce;
    public float maxSpeed;
    public float patrolingMaxSpeed;
    public float acceleration;
    public float airControl;
    public float jumpTriggerAngle;
    [Space]
    public float damage;
    public float attackKnockBackForce;
    public float attackKnockBackUp;
    public float attackStunTime;
    [Space]
    public float jumpCooldown;
    public Vector2 jumpAttackColliderSize;
    public float chargeTime;
    public GameObject chargingParticlePrefab;
    public float jumpAttackTriggerDistance;
    public Vector2 jumpAttackForce;

    private bool isInrange;
    private GameObject chargeParticle;
    private float currentChargedTime;
    private float jumpCooldownRemaining;
    private float pathDirectionAngle;
    private int xDirection;
    private bool isAttackJumping;
    private bool isChargingJump;

    private float patrolDirection;
    private bool fleeTargetPos;
    private float fleeDirection;
    private float currentMaxSpeed;

    private void Start()
    {
        HandlerStart();

        jumpCooldownRemaining = 0;
        isAttackJumping = false;
        patrolDirection = 0;
        pathDirectionAngle = 0;
    }

    private void Update()
    {
        HandlerUpdate();

        UpdateFleeDirection();

        ProvocationUpdate();
    }

    private void FixedUpdate()
    {
        HandlerFixedUpdate();

        UpdateMovement();

        Behavior();
    }

    public override void UpdateMovement()
    {
        if (!Is(Effect.Stun) && !Is(Effect.NoControl) && !Is(Effect.NoGravity) && !isChargingJump)
        {
            if(provoked)
            {
                currentMaxSpeed = maxSpeed;
                if(isInrange)
                {
                    fleeTargetPos = true;
                }
                else
                {
                    fleeTargetPos = false;
                }

                patrolDirection = 0;
                if (currentPlatform != null && currentPlatform == GameData.playerMovement.currentPlayerPlatform)
                {
                    targetPathfindingPosition = GameData.playerAttackManager.transform.position;
                }
                else if (targetConnection != null)
                {
                    targetPathfindingPosition = targetConnection.transform.position;
                    if (Vector2.Distance(feetPos.position, targetConnection.transform.position) < 0.5f && pJumpCDRemaining <= 0)
                    {
                        StartCoroutine(JumpToConnection(targetConnectedConnection));
                    }
                }
            }
            else
            {
                currentMaxSpeed = patrolingMaxSpeed;
                fleeTargetPos = false;
                if(patrolDirection == 0)
                {
                    patrolDirection = Mathf.Sign(Random.Range(-1.0f, 1.0f));
                }

                if(IsNearAnEdge(1) && isOnGround)
                {
                    patrolDirection = -patrolDirection;
                }

                targetPathfindingPosition = new Vector2(transform.position.x + patrolDirection, transform.position.y);
            }

            if (targetPathfindingPosition != null)
            {
                float addedVelocity = Mathf.Sign(targetPathfindingPosition.x - transform.position.x) * fleeDirection * Time.fixedDeltaTime;

                if (isOnGround)
                {
                    addedVelocity *= acceleration;
                }
                else
                {
                    addedVelocity *= airControl;
                }

                if (addedVelocity > 0)
                {
                    facingRight = true;
                }
                else
                {
                    facingRight = false;
                }

                rb.velocity = new Vector2(rb.velocity.x + addedVelocity, rb.velocity.y);

                if (Mathf.Abs(rb.velocity.x) > currentMaxSpeed * slowEffectScale)
                {
                    rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * currentMaxSpeed * slowEffectScale, rb.velocity.y);
                }
            }
            #region A* Pathfinding example
            /*if (path != null && !pathEndReached)
            {
                if (isOnGround)
                {
                    rb.velocity = new Vector2(rb.velocity.x + pathDirection.x * acceleration * Time.fixedDeltaTime, rb.velocity.y);

                    pathDirectionAngle = -90;
                    if ((currentWaypoint) < path.vectorPath.Count)
                    {
                        pathDirectionAngle = Vector2.SignedAngle(Vector2.right, path.vectorPath[currentWaypoint] - transform.position);
                    }

                    if (pathDirectionAngle > 90 - jumpTriggerAngle / 2 && pathDirectionAngle < 90 + jumpTriggerAngle / 2)
                    {
                        Propel(Vector2.up * jumpForce, false, true);
                    }
                }
                else
                {
                    rb.velocity = new Vector2(rb.velocity.x + pathDirection.x * airControl * Time.fixedDeltaTime, rb.velocity.y);
                }

                if (Mathf.Abs(rb.velocity.x) > maxSpeed)
                {
                    rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
                }
            }*/
            #endregion
        }
        else
        {
            if (Mathf.Abs(rb.velocity.x) > acceleration * Time.fixedDeltaTime && isOnGround)
            {
                rb.velocity = new Vector2(rb.velocity.x - Mathf.Sign(rb.velocity.x) * acceleration * Time.fixedDeltaTime, rb.velocity.y);
            }
            else if (Mathf.Abs(rb.velocity.x) <= acceleration * Time.fixedDeltaTime && isOnGround)
            {
                rb.velocity = new Vector2(0.0f, rb.velocity.y);
            }
        }
    }

    private void Behavior()
    {
        if (jumpCooldownRemaining > 0)
        {
            jumpCooldownRemaining -= Time.fixedDeltaTime;
        }

        if(isAttackJumping)
        {
            if(!Is(Effect.Stun) && IsTouchingPlayer())
            {
                Attack();
            }

            if(isOnGround && jumpCooldownRemaining > 0.2f)
            {
                isAttackJumping = false;
                isInvulnerable = false;
            }
        }

        xDirection = (int)Mathf.Sign(GameData.playerMovement.transform.position.x - transform.position.x);
        if ((pathDirectionAngle < 45 || pathDirectionAngle > 135) && Physics2D.OverlapBox(new Vector2(transform.position.x + xDirection * jumpAttackTriggerDistance / 2, transform.position.y), new Vector2(jumpAttackTriggerDistance, 1.0f), 0.0f, LayerMask.GetMask("Player")))
        {
            isInrange = true;
        }
        else if(isInrange)
        {
            isInrange = false;
        }
        
        if(jumpCooldownRemaining <= 0 && provoked && isInrange && isOnGround && !Is(Effect.Stun) && !Is(Effect.Hack) && !Is(Effect.NoGravity))
        {
            if(!isChargingJump && !Is(Effect.NoControl))
            {
                currentChargedTime = 0;
                isChargingJump = true;
                chargeParticle = Instantiate(chargingParticlePrefab, transform);
            }
            else if(isChargingJump)
            {
                currentChargedTime += Time.fixedDeltaTime * slowEffectScale;

                if (currentChargedTime > chargeTime)
                {
                    isChargingJump = false;
                    Destroy(chargeParticle);
                    Propel(new Vector2(xDirection * jumpAttackForce.x, jumpAttackForce.y), true, true);
                    SetEffect(Effect.NoControl, 0.5f, false);
                    isAttackJumping = true;
                    isInvulnerable = true;
                    jumpCooldownRemaining = jumpCooldown;
                }
            }
        }
        else if(currentChargedTime > 0)
        {
            isChargingJump = false;
            Destroy(chargeParticle);
        }
    }

    public override bool TestCounter()
    {
        bool countering = false;

        if(isAttackJumping)
        {
            countering = true;
            Attack();
        }

        return countering;
    }

    private void Attack()
    {
        Vector2 knockBack = (GameData.playerMovement.transform.position - transform.position).normalized * attackKnockBackForce;
        knockBack.y += attackKnockBackUp;
        if(GameData.playerManager.TakeDamage(damage, knockBack, attackStunTime))
        {
            Propel(-knockBack * 0.5f, true, true);
        }
    }

    private bool IsTouchingPlayer()
    {
        bool isTouchingPlayer = false;
        Collider2D collider = Physics2D.OverlapBox(transform.position, jumpAttackColliderSize, 0.0f, LayerMask.GetMask("Player"));
        if (collider != null)
        {
            isTouchingPlayer = true;
        }
        return isTouchingPlayer;
    }

    private void UpdateFleeDirection()
    {
        if(fleeTargetPos && fleeDirection != -1)
        {
            fleeDirection = -1;
        }
        else if(!fleeTargetPos && fleeDirection != 1)
        {
            fleeDirection = 1;
        }
    }

    private void ProvocationUpdate()
    {
        if (!provoked)
        {
            if (PlayerInSight() && !Is(Effect.NoControl) && !Is(Effect.Stun))
            {
                provoked = true;
            }
        }
        else if(Vector2.Distance(transform.position, GameData.playerMovement.transform.position) > agroRange)
        {
            provoked = false;
        }
    }
}
