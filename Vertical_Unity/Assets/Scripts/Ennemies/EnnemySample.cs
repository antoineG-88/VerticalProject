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

    private float cooldownRemaining;

    private void Start()
    {
        HandlerStart();

        cooldownRemaining = 0;
    }

    private void Update()
    {
        UpdateMovement();

        Behavior();
    }

    public override void UpdateMovement()
    {
        HandlerUpdate();
        if(!isStunned && isInControl)
        {
            if (path != null && !pathEndReached)
            {
                if(IsOnGround())
                {
                    rb.velocity = new Vector2(rb.velocity.x + pathDirection.x * acceleration * Time.deltaTime, rb.velocity.y);

                    float pathDirectionAngle = -90;
                    if ((currentWaypoint + 2) < path.vectorPath.Count)
                    {
                        pathDirectionAngle = Vector2.SignedAngle(Vector2.right, path.vectorPath[currentWaypoint + 2] - transform.position);
                    }
                    previsuDirection.transform.localRotation = Quaternion.Euler(0, 0, pathDirectionAngle - 90);
                    if (pathDirectionAngle > 90 - jumpTriggerAngle / 2 && pathDirectionAngle < 90 + jumpTriggerAngle / 2)
                    {
                        Propel(Vector2.up * jumpForce, false, true);
                    }
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
            if(Mathf.Abs(rb.velocity.x) > 0 && IsOnGround())
            {
                rb.velocity = new Vector2(rb.velocity.x - Mathf.Sign(rb.velocity.x) * acceleration * Time.deltaTime, rb.velocity.y);
            }
        }

        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y - gravityForce * Time.deltaTime);
    }

    public void Behavior()
    {
        if(cooldownRemaining > 0)
        {
            cooldownRemaining -= Time.deltaTime;
        }

        if(isTouchingPlayer && cooldownRemaining <= 0 && !GameData.playerGrapplingHandler.isTracting && !isStunned)
        {
            Vector2 knockBack = (GameData.playerMovement.transform.position - transform.position).normalized * attackKnockBackForce;
            knockBack.y += attackKnockBackUp;
            GameData.playerManager.TakeDamage(damage, knockBack, attackStunTime);
            cooldownRemaining = attackCooldown;
        }
    }
}
