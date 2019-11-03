using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class EnnemySample : EnnemyHandler
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
    public float attackCooldown;
    public float jumpCooldown;
    [Space]
    public float jumpAttackTriggerDistance;
    public Vector2 jumpAttackForce;
    public float attackPauseTime;

    private float cooldownRemaining;
    private float jumpCooldownRemaining;
    private float pathDirectionAngle;
    private int horiz;

    private void Start()
    {
        HandlerStart();

        cooldownRemaining = 0;
        jumpCooldownRemaining = 0;
    }

    private void Update()
    {
        HandlerUpdate();

        UpdateMovement();

        Behavior();

        if(Input.GetKeyDown(KeyCode.P))
        {
            if(currentPlatform.IsUnder(GameData.playerManager.gameObject))
            {
                Debug.Log("Player is on the same platform as " + gameObject.name);
            }
        }
    }

    private void FixedUpdate()
    {
        HandlerFixedUpdate();
    }

    public override void UpdateMovement()
    {
        if (!isStunned && isInControl)
        {
            if(IsOnGround() && currentPlatform.IsUnder(GameData.playerManager.gameObject))
            {
                targetPathfindingPosition = GameData.playerAttackManager.transform.position;
            }
            else if (targetConnection != null)
            {
                targetPathfindingPosition = targetConnection.transform.position;
                if(Vector2.Distance(feetPos.position, targetConnection.transform.position) < 0.5f)
                {
                    StartCoroutine(JumpToConnection(targetConnectedConnection));
                }
            }

            if (path != null && !pathEndReached)
            {
                if(IsOnGround())
                {
                    rb.velocity = new Vector2(rb.velocity.x + pathDirection.x * acceleration * Time.deltaTime, rb.velocity.y);

                    pathDirectionAngle = -90;
                    if ((currentWaypoint) < path.vectorPath.Count)
                    {
                        pathDirectionAngle = Vector2.SignedAngle(Vector2.right, path.vectorPath[currentWaypoint] - transform.position);
                    }
                    previsuDirection.transform.localRotation = Quaternion.Euler(0, 0, pathDirectionAngle - 90);
                    /*if (pathDirectionAngle > 90 - jumpTriggerAngle / 2 && pathDirectionAngle < 90 + jumpTriggerAngle / 2)
                    {
                        Propel(Vector2.up * jumpForce, false, true);
                    }*/
                }
                else
                {
                    rb.velocity = new Vector2(rb.velocity.x + pathDirection.x * airControl * Time.deltaTime, rb.velocity.y);
                }

                if (Mathf.Abs(rb.velocity.x) > maxSpeed)
                {
                    rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
                }
            }
        }
        else
        {
            if(Mathf.Abs(rb.velocity.x) > 0.2f && IsOnGround())
            {
                rb.velocity = new Vector2(rb.velocity.x - Mathf.Sign(rb.velocity.x) * acceleration * Time.deltaTime, rb.velocity.y);
            }
            else if(Mathf.Abs(rb.velocity.x) <= 0.2f && IsOnGround())
            {
                rb.velocity = new Vector2(0.0f, rb.velocity.y);
            }
        }

        if(isAffectedByGravity)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y - gravityForce * Time.deltaTime);
        }
    }

    public void Behavior()
    {
        if(cooldownRemaining > 0)
        {
            cooldownRemaining -= Time.deltaTime;
        }

        if (jumpCooldownRemaining > 0)
        {
            jumpCooldownRemaining -= Time.deltaTime;
        }

        if (isTouchingPlayer && cooldownRemaining <= 0 && !GameData.playerGrapplingHandler.isTracting && !isStunned)
        {
            Vector2 knockBack = (GameData.playerMovement.transform.position - transform.position).normalized * attackKnockBackForce;
            knockBack.y += attackKnockBackUp;
            GameData.playerManager.TakeDamage(damage, knockBack, attackStunTime);
            cooldownRemaining = attackCooldown;
            rb.velocity = -knockBack * 0.5f;
        }

        horiz = (int)Mathf.Sign(GameData.playerMovement.transform.position.x - transform.position.x);
        if(jumpCooldownRemaining <= 0 && (pathDirectionAngle < 45 || pathDirectionAngle > 135) && Physics2D.OverlapBox(new Vector2(transform.position.x + horiz * jumpAttackTriggerDistance / 2, transform.position.y),new Vector2(jumpAttackTriggerDistance, 1.0f),0.0f,LayerMask.GetMask("Player")) && IsOnGround() && !isStunned)
        {
            Propel(new Vector2(horiz * jumpAttackForce.x, jumpAttackForce.y), true, true);
            StartCoroutine(NoControl(0.5f));
            jumpCooldownRemaining = jumpCooldown;
        }
    }
}
