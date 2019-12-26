using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchBot : EnemyHandler
{
    [Header("WatchBot settings")]
    public float acceleration;
    public float maxSpeed;
    public float slowingForce;
    public float timeBetweenPatrolMoves;
    public float patrolRadius;
    public float stopDistance;
    public float fleeDistance;
    [Header("Range-attack settings")]
    public float rangeAttackTriggerRange;
    public int projectileNumber;
    public float attackWidthAngle;
    public float projectileSpeed;
    public int projectileDamage;
    public float rangeAttackCooldown;
    public float rangeAttackDelay;
    public float projectileSpawnDistance;
    public float projectileKnockbackForce;
    public float projectileLifeTime;
    [Header("Spike-attack settings")]
    public float spikeAttackRange;
    public float spikeAttackDelay;
    public int spikeAttackDamage;
    public float spikeAttackKnockbackForce;
    [Header("Debug settings")]
    public GameObject particleDebugPrefab;
    public GameObject projectilePrefab;

    private Vector2 patrolCenterPosition;
    private bool isPatroling;
    [HideInInspector] public bool isAtRange;
    private bool targetReached;
    private bool isFleeing;
    [HideInInspector] public float timebeforeRangeAttack;
    [HideInInspector] public float rangeAttackCooldownRemaining;
    private Vector2 aimDirection;

    private void Start()
    {
        HandlerStart();

        isPatroling = false;
        provoked = false;
        isAtRange = false;
        isFleeing = false;

        timebeforeRangeAttack = 0;
        rangeAttackCooldownRemaining = 0;
    }

    private void Update()
    {
        HandlerUpdate();

        ProvocationUpdate();

        Behavior();
    }

    private void FixedUpdate()
    {
        HandlerFixedUpdate();

        UpdateMovement();
    }

    public override void UpdateMovement()
    {
        if (!Is(Effect.Stun) && !Is(Effect.NoControl) && !Is(Effect.NoGravity))
        {
            if(!targetReached)
            {
                if (provoked)
                {
                    if(!isFleeing)
                    {
                        targetPathfindingPosition = GameData.playerMovement.gameObject.transform.position;
                    }
                    else
                    {
                        targetPathfindingPosition = transform.position + (transform.position - GameData.playerMovement.transform.position).normalized * 2;
                    }
                }
                else if (!isPatroling)
                {
                    StartCoroutine(Patrol());
                }


                if (path != null && !pathEndReached)
                {
                    Vector2 addedVelocity = new Vector2(pathDirection.x * acceleration, pathDirection.y * acceleration);

                    rb.velocity += addedVelocity * Time.fixedDeltaTime;

                    if (addedVelocity.magnitude > maxSpeed * slowEffectScale)
                    {
                        rb.velocity = rb.velocity.normalized * maxSpeed * slowEffectScale;
                    }
                }

                transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, Vector2.SignedAngle(Vector2.right, pathDirection)));
            }
            else
            {
                if(rb.velocity.magnitude > slowingForce * Time.fixedDeltaTime)
                {
                    rb.velocity -= rb.velocity * slowingForce * Time.fixedDeltaTime;
                }
                else
                {
                    rb.velocity = Vector2.zero;
                }
            }
        }

        if(isPatroling)
        {
            if(Vector2.Distance(transform.position, targetPathfindingPosition) < stopDistance)
            {
                targetReached = true;
            }
            else
            {
                targetReached = false;
            }
        }
        else
        {
            if(isAtRange)
            {
                targetReached = true;
            }
            else
            {
                targetReached = false;
            }
        }
    }

    private void Behavior()
    {
        aimDirection = (GameData.playerMovement.transform.position - transform.position).normalized;

        if (isAtRange && !Is(Effect.Stun) && !Is(Effect.Hack) && !Is(Effect.NoControl))
        {
            transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, Vector2.SignedAngle(Vector2.right, aimDirection)));

            if (rangeAttackCooldownRemaining <= 0)
            {
                timebeforeRangeAttack -= Time.deltaTime;
                if (timebeforeRangeAttack <= 0)
                {
                    RangeAttack();
                }
            }
        }
        else
        {
            timebeforeRangeAttack = rangeAttackDelay;
        }

        if (rangeAttackCooldownRemaining > 0)
        {
            rangeAttackCooldownRemaining -= Time.deltaTime;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, GameData.playerMovement.transform.position);

        isAtRange = distanceToPlayer < rangeAttackTriggerRange && distanceToPlayer > fleeDistance && provoked ? true : false;
        isFleeing = distanceToPlayer < fleeDistance && provoked ? true : false;

        if(distanceToPlayer < spikeAttackRange && !Is(Effect.NoControl))
        {
            StartCoroutine(SpikeAttack());
        }
    }

    private void RangeAttack()
    {
        rangeAttackCooldownRemaining = rangeAttackCooldown;

        float subAngle = attackWidthAngle / (projectileNumber - 1);
        float firstAngle = - attackWidthAngle / 2;
        for (int i = 0; i < projectileNumber; i++)
        {
            float relativeAngle = firstAngle + subAngle * i;
            float angledDirection = Vector2.SignedAngle(Vector2.right, aimDirection) + relativeAngle;
            Vector2 direction = new Vector2(Mathf.Cos((angledDirection) * Mathf.Deg2Rad), Mathf.Sin((angledDirection) * Mathf.Deg2Rad));

            Vector2 spawnPos = (Vector2)transform.position + direction * projectileSpawnDistance;
            GameObject newProjectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            ProjectileHandler newProjectileHandler = newProjectile.GetComponent<ProjectileHandler>();
            newProjectileHandler.initialVelocity = direction * projectileSpeed;
            newProjectileHandler.knockBackForce = projectileKnockbackForce;
            newProjectileHandler.destroyTime = 0;
            newProjectileHandler.damage = projectileDamage;
            newProjectileHandler.lifeTime = projectileLifeTime;
        }
    }

    private IEnumerator SpikeAttack()
    {
        SetEffect(Effect.NoControl, spikeAttackDelay, false);
        yield return new WaitForSeconds(spikeAttackDelay);
        if (Physics2D.OverlapBox((Vector2)transform.position + aimDirection * spikeAttackRange / 2, new Vector2(spikeAttackRange, 0.5f), Vector2.SignedAngle(Vector2.right, aimDirection), LayerMask.GetMask("Player")) && !GameData.playerGrapplingHandler.isTracting)
        {
            GameData.playerManager.TakeDamage(spikeAttackDamage, aimDirection * spikeAttackKnockbackForce, 0.5f);
        }
    }

    public override bool TestCounter()
    {
        return false;
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
        else if (Vector2.Distance(transform.position, GameData.playerMovement.transform.position) > agroRange)
        {
            provoked = false;
            Debug.Log(gameObject.name + " unprovoked because too far");
        }
    }

    private IEnumerator Patrol()
    {
        patrolCenterPosition = transform.position;
        Vector2 nextPatrolSpot;
        isPatroling = true;

        while (!provoked)
        {
            int attempt = 20;
            nextPatrolSpot = Vector2.zero;
            while (nextPatrolSpot == Vector2.zero && attempt > 0)
            {
                attempt--;
                Vector2 direction = new Vector2(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)).normalized;
                float distance = Random.Range(0.0f, patrolRadius);

                if (!Physics2D.OverlapPoint(patrolCenterPosition + direction * distance, LayerMask.GetMask("Ground")))
                {
                    nextPatrolSpot = patrolCenterPosition + direction * distance;
                }
            }

            if(nextPatrolSpot != Vector2.zero)
            {
                targetPathfindingPosition = nextPatrolSpot;
            }

            yield return new WaitForSeconds(timeBetweenPatrolMoves);
        }

        isPatroling = false;
    }
}
