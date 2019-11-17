using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class EnemySample : EnemyHandler
{
    public GameObject previsuDirection;
    [Header("EnnemySample settings")]
    public float jumpForce;
    public float maxSpeed;
    public float acceleration;
    public float airControl;
    public float jumpTriggerAngle;
    public float gravityForce;
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
    public float gameRange;
    public Vector2 jumpAttackForce;

    private bool isInrange;
    private GameObject chargeParticle;
    private float currentChargedTime;
    private float jumpCooldownRemaining;
    private float pathDirectionAngle;
    private int xDirection;
    private bool isAttackJumping;
    private bool isChargingJump;

    private void Start()
    {
        HandlerStart();

        jumpCooldownRemaining = 0;
        isAttackJumping = false;
    }

    private void Update()
    {
        HandlerUpdate();
    }

    private void FixedUpdate()
    {
        HandlerFixedUpdate();

        UpdateMovement();

        Behavior();
    }

    public override void UpdateMovement()
    {
        if (!Is(Effect.Stun) && !Is(Effect.NoControl) && !isChargingJump && !isInrange)
        {
            if(currentPlatform != null && currentPlatform == GameData.playerMovement.currentPlayerPlatform)
            {
                targetPathfindingPosition = GameData.playerAttackManager.transform.position;
            }
            else if (targetConnection != null)
            {
                targetPathfindingPosition = targetConnection.transform.position;
                if(Vector2.Distance(feetPos.position, targetConnection.transform.position) < 0.5f && pJumpCDRemaining <= 0)
                {
                    StartCoroutine(JumpToConnection(targetConnectedConnection));
                }
            }

            if(targetPathfindingPosition != null)
            {
                int direction = 1;
                if(isInrange)
                {
                    direction = -1;
                }

                if(IsOnGround())
                {
                    rb.velocity = new Vector2(rb.velocity.x + acceleration * Mathf.Sign(targetPathfindingPosition.x - transform.position.x) * direction * Time.fixedDeltaTime, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(rb.velocity.x + airControl * Mathf.Sign(targetPathfindingPosition.x - transform.position.x) * direction * Time.fixedDeltaTime, rb.velocity.y);
                }

                if (Mathf.Abs(rb.velocity.x) > maxSpeed)
                {
                    rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
                }
            }

            if (path != null && !pathEndReached)
            {
                if(IsOnGround())
                {
                    rb.velocity = new Vector2(rb.velocity.x + pathDirection.x * acceleration * Time.fixedDeltaTime, rb.velocity.y);

                    pathDirectionAngle = -90;
                    if ((currentWaypoint) < path.vectorPath.Count)
                    {
                        pathDirectionAngle = Vector2.SignedAngle(Vector2.right, path.vectorPath[currentWaypoint] - transform.position);
                    }
                    previsuDirection.transform.localRotation = Quaternion.Euler(0, 0, pathDirectionAngle - 90);
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
            }
        }
        else
        {
            if(Mathf.Abs(rb.velocity.x) > acceleration * Time.fixedDeltaTime && IsOnGround())
            {
                rb.velocity = new Vector2(rb.velocity.x - Mathf.Sign(rb.velocity.x) * acceleration * Time.fixedDeltaTime, rb.velocity.y);
            }
            else if(Mathf.Abs(rb.velocity.x) <= acceleration * Time.fixedDeltaTime && IsOnGround())
            {
                rb.velocity = new Vector2(0.0f, rb.velocity.y);
            }
        }

        if(isAffectedByGravity)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y - gravityForce * Time.fixedDeltaTime);
        }
    }

    public void Behavior()
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

            if(IsOnGround() && jumpCooldownRemaining > 0.2f)
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
        
        if(jumpCooldownRemaining <= 0 && isInrange && IsOnGround() && !Is(Effect.Stun))
        {
            if(!isChargingJump && !Is(Effect.NoControl))
            {
                currentChargedTime = 0;
                isChargingJump = true;
                chargeParticle = Instantiate(chargingParticlePrefab, transform);
            }
            else if(isChargingJump)
            {
                currentChargedTime += Time.fixedDeltaTime;

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

    public bool IsTouchingPlayer()
    {
        bool isTouchingPlayer = false;
        Collider2D collider = Physics2D.OverlapBox(transform.position, jumpAttackColliderSize, 0.0f, LayerMask.GetMask("Player"));
        if (collider != null)
        {
            isTouchingPlayer = true;
        }
        return isTouchingPlayer;
    }
}
