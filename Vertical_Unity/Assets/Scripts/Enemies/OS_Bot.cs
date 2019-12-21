using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OS_Bot : EnemyHandler
{
    [Header("OS_Bot settings")]
    public float acceleration;
    public float maxSpeed;
    public float slowingForce;
    public float timeBetweenPatrolMoves;
    public float patrolRadius;
    public float stopDistance;
    public float fleeDistance;
    [Header("Snip-attack settings")]
    public float snipAttackCooldown;
    public float snipAttackLockRange;
    public float lockingTime;
    public float chargingTime;
    public int laserDamage;
    public float laserBeamTime;
    public float laserStunTime;
    [Header("Debug settings")]
    public GameObject particleDebugPrefab;
    public GameObject projectilePrefab;

    private Vector2 patrolCenterPosition;
    private bool isPatroling;
    [HideInInspector] public bool isAtRange;
    private bool targetReached;
    private bool isFleeing;
    private float lockTimeRemaining;
    [HideInInspector] public float snipAttackCooldownRemaining;
    private Vector2 aimDirection;
    private LineRenderer laserLockLine;
    [HideInInspector] public bool isCharging;

    private void Start()
    {
        HandlerStart();
        isCharging = false;
        isPatroling = false;
        provoked = false;
        isAtRange = false;
        isFleeing = false;
        laserLockLine = GetComponent<LineRenderer>();
        laserLockLine.enabled = false;
        lockTimeRemaining = 0;
        snipAttackCooldownRemaining = 0;
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

        RaycastHit2D hit = Physics2D.Raycast(transform.position, aimDirection, snipAttackLockRange, LayerMask.GetMask("Player", "Ground"));

        if (isAtRange && !Is(Effect.Stun) && !Is(Effect.Hack) && hit && hit.collider.CompareTag("Player") && snipAttackCooldownRemaining <= 0)
        {
            SetEffect(Effect.NoControl, 0.2f, false);
            transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, Vector2.SignedAngle(Vector2.right, aimDirection)));
            lockTimeRemaining -= Time.deltaTime;
            laserLockLine.enabled = true;
            laserLockLine.SetPosition(0, transform.position);
            laserLockLine.SetPosition(1, GameData.playerMovement.transform.position);
            laserLockLine.startWidth = 0.1f;
            laserLockLine.endWidth = 0.1f;

            if (lockTimeRemaining <= 0)
            {
                StartCoroutine(SnipAttack());
            }
        }
        else
        {
            lockTimeRemaining = lockingTime;
            if(!isCharging)
            {
                laserLockLine.enabled = false;
            }
        }

        if (snipAttackCooldownRemaining > 0)
        {
            snipAttackCooldownRemaining -= Time.deltaTime;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, GameData.playerMovement.transform.position);

        isAtRange = distanceToPlayer < snipAttackLockRange && distanceToPlayer > fleeDistance && provoked ? true : false;
        isFleeing = distanceToPlayer < fleeDistance && provoked ? true : false;

    }

    private IEnumerator SnipAttack()
    {
        snipAttackCooldownRemaining = snipAttackCooldown;
        Vector2 direction = aimDirection;
        float timer = chargingTime;
        laserLockLine.startWidth = 0.3f;
        laserLockLine.endWidth = 0.3f;
        isCharging = true;
        while (timer > 0 && !Is(Effect.Stun) && !Is(Effect.Hack))
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 100.0f, LayerMask.GetMask("Player", "Ground"));
            laserLockLine.enabled = true;
            laserLockLine.SetPosition(0, transform.position);
            laserLockLine.SetPosition(1, hit.point);
            timer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        if (timer <= 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 100.0f, LayerMask.GetMask("Ground"));
            laserLockLine.startWidth = 1f;
            laserLockLine.endWidth = 1f;
            laserLockLine.enabled = true;
            laserLockLine.SetPosition(0, transform.position);
            laserLockLine.SetPosition(1, hit.point);
            float laserTimer = laserBeamTime;

            while (laserTimer > 0)
            {
                RaycastHit2D playerHit = Physics2D.Raycast(transform.position, direction, 100.0f, LayerMask.GetMask("Player"));
                if(playerHit)
                {
                    GameData.playerManager.TakeDamage(laserDamage, Vector2.zero, laserStunTime);
                }

                laserTimer -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            laserLockLine.enabled = false;
            isCharging = false;
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
