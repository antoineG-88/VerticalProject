using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public abstract class EnemyHandler : MonoBehaviour
{
    [Header("Enemy basic settings")]
    public new string name;
    public int maxHealth;
    public Color hurtColor;
    public float invulnerableTime;
    public float gravityForce;
    public float deathAnimationTime;
    [Header("NoGravity Effect settings")]
    public float noGravityLinearDrag;
    public float noGravityAngularDrag;
    public float noGravityRiseUpForce;
    public float noGravityRiseUpAngularRange;
    public PhysicsMaterial2D noGravityBouncyMaterial;
    public PhysicsMaterial2D basicMaterial;
    [Header("Magnetism Effect settings")]
    public float repulsionForce;
    public float repulsionRange;
    [Header("Provocation settings")]
    public float viewRange;
    public float viewAngle;
    public float viewAngleWidth;
    public float raycastNumber;
    public float agroRange;
    [Header("Enemy pathfinding settings")]
    public bool useAStar;
    public float nextWaypointDistance;
    public int waypointAhead;
    [Space]
    public float pathUpdatingFrequency;
    public float connectionJumpTime;
    public float pathfindingJumpWeight;
    public float pathFindingJumpCooldown;
    [Space]
    public float avoidEffectRadius;
    public float avoidForce;
    public float minimalAvoidForce;
    public float maximalAvoidForce;
    public float avoidDistanceInfluence;
    [Header("Enemy technical settings")]
    public Transform feetPos;
    public float groundCheckWidth;
    public float groundCheckThickness;
    public LayerMask groundMask;
    public Collider2D collisionCollider;
    [Space]
    [Header("Debug settings")]
    public bool pauseAI;
    public GameObject debugParticlePrefab;
    public bool showFieldOfView;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public bool isInvulnerable; // (à modifié)
    [HideInInspector] public bool playerInSight; // inutile
    [HideInInspector] public bool provoked; //défint si l'ennemi est provoqué (à modifié)
    [HideInInspector] public bool facingRight;
    [HideInInspector] public bool isOnGround;
    [HideInInspector] public bool avoidEnemies;
    [HideInInspector] public bool isDead;

    [HideInInspector] public float slowEffectScale; //a mettre en multiplicateur sur les variables affecté par le slow
    [HideInInspector] public float[] currentEffects;
    [HideInInspector] public GameObject[] currentEffectFx;
    [HideInInspector] public bool isAffectedByGravity;
    [HideInInspector] public Rigidbody2D rb; //rigidbody déjà intégré


    [HideInInspector] public Seeker seeker;
    [HideInInspector] public Path path;
    [HideInInspector] public int currentWaypoint;
    [HideInInspector] public bool pathEndReached;
    [HideInInspector] public Vector2 pathDirection; //La direction du chemin de pathfinding (à lire)
    [HideInInspector] public Vector2 targetPathfindingPosition; //targetPathfindingPosition détermine ou la cible du pathfinding (a modifié)

    [HideInInspector] public PlatformHandler currentPlatform;
    [HideInInspector] public PlatformConnection targetConnection;
    [HideInInspector] public PlatformConnection targetConnectedConnection;
    [HideInInspector] public float timeBeforeNextPathUpdate;
    private List<PlatformHandler> testedPlatform = new List<PlatformHandler>();
    [HideInInspector] public float pJumpCDRemaining;

    private Color baseColor;

    public void HandlerStart()
    {
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();

        baseColor = GetComponentInChildren<SpriteRenderer>().color;
        pathEndReached = false;
        isInvulnerable = false;
        isAffectedByGravity = true;
        provoked = false;
        playerInSight = false;
        avoidEnemies = true;
        isDead = false;
        currentHealth = maxHealth;
        pJumpCDRemaining = 0;
        currentEffects = new float[GameData.gameController.enemyEffects.Count];
        currentEffectFx = new GameObject[GameData.gameController.enemyEffects.Count];
        for (int i = 0; i < currentEffects.Length; i++)
        {
            currentEffects[i] = 0;
        }

        timeBeforeNextPathUpdate = 0;
    }

    public void HandlerUpdate()
    {
        UpdatePath();

        EffectUpdate();

        if(Input.GetKeyDown(KeyCode.A))
        {
            SetEffect(Effect.NoGravity, 1, true);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            slowEffectScale = 0.5f;
            SetEffect(Effect.Slow, 1, true);
        }
    }

    public void HandlerFixedUpdate()
    {
        UpdateGravity();

        UpdateMagnetism();

        AvoidOtherEnemies();

        isOnGround = IsOnGround();
    }

    #region Movement


    public void CalculatePath()
    {
        seeker.StartPath(transform.position, targetPathfindingPosition, OnPathComplete);
    }

    private void OnPathComplete(Path p)
    {
        if(!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    public PlatformHandler GetCurrentPlatform()
    {
        PlatformHandler platform = null;
        Collider2D collider = Physics2D.OverlapCircle(feetPos.position, 1.2f, LayerMask.GetMask("Ground"));
        if(collider != null && (currentPlatform == null || currentPlatform.gameObject != collider.gameObject))
        {
            //Debug.Log("Set current platform for " + gameObject.name + " to " + collider.gameObject.name);
            platform = collider.GetComponent<PlatformHandler>();
        }

        return platform;
    }

    public bool FindGroundPathToTarget(GameObject target)
    {
        bool playerFound = false;

        //Debug.Log("GROUND PATHFINDING STARTED");

        PlatformHandler platformFound = GetCurrentPlatform();
        if (platformFound != null)
        {
            currentPlatform = platformFound;
        }

        testedPlatform.Clear();
        testedPlatform.Add(currentPlatform);

        GameData.playerMovement.currentPlayerPlatform = null;

        if (FindNextConnection(currentPlatform, target, feetPos.position) > 0)
        {
            playerFound = true;
            //Debug.Log("Player accessible !");
        }
        //Instantiate(debugParticlePrefab, targetConnection.transform.position, Quaternion.identity);

        return playerFound;
    }


    /// <summary>
    /// Change the value of targetConnection and targetConnectedConnection.
    /// Return the distance between the connected connection and the next connection in negative, if positive it's the distance between the player and the connected connection
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="target"></param>
    /// <param name="originConnectionPos"></param>
    /// <param name="linkedConnection"></param>
    /// <returns></returns>
    private float FindNextConnection(PlatformHandler platform, GameObject target, Vector2 originConnectionPos)
    {
        //Debug.Log("Testing player presence on " + platform.gameObject.name);

        if (GameData.playerMovement.currentPlayerPlatform == null && platform.IsUnder(target))
        {
            GameData.playerMovement.currentPlayerPlatform = platform;
        }

        if(platform == GameData.playerMovement.currentPlayerPlatform)
        {
            //Debug.Log("Player found on " + platform.gameObject.name + " !");

            float distanceToPlayer = Vector2.Distance(originConnectionPos, target.transform.position);
            return distanceToPlayer;
        }
        else
        {
            //Debug.Log("No player found. Now testing connections on " + platform.gameObject.name);

            float minDistanceBetweenConnection = -1;

            foreach (PlatformConnection connection in platform.connections)
            {
                //Debug.Log("Testing " + connection.gameObject.name);
                if(connection.connectedConnections.Count > 0)
                {
                    //Debug.Log(connection.gameObject.name + " has at least 1 connected connection");

                    float minDistanceBetweenConnected = -1;
                    float distanceToConnection = Vector2.Distance(originConnectionPos, connection.transform.position) + pathfindingJumpWeight;
                    PlatformConnection tempCC = null;

                    for (int i = 0; i < connection.connectedConnections.Count; i++)
                    {
                        //Debug.Log("Testing connected " + connection.connectedConnections[i].gameObject.name);
                        if (!testedPlatform.Contains(connection.connectedConnections[i].attachedPlatformHandler))
                        {
                            testedPlatform.Add(connection.connectedConnections[i].attachedPlatformHandler);
                            
                            float distanceFromEnd = FindNextConnection(connection.connectedConnections[i].attachedPlatformHandler, target, connection.connectedConnections[i].transform.position);

                            if (distanceFromEnd > 0)
                            {
                                testedPlatform.Remove(connection.connectedConnections[i].attachedPlatformHandler);
                                //Debug.Log("Player found at " + distanceFromEnd + " passing by " + connection.connectedConnections[i].gameObject.name + " linked with " + connection.gameObject.name);
                                if (distanceFromEnd < minDistanceBetweenConnected || minDistanceBetweenConnected == -1)
                                {
                                    //Debug.Log("Better CC found with " + distanceFromEnd + " instead of " + minDistanceBetweenConnected);
                                    minDistanceBetweenConnected = distanceFromEnd;
                                    tempCC = connection.connectedConnections[i];
                                }
                            }
                        }
                        else
                        {
                            //Debug.Log(connection.connectedConnections[i].attachedPlatformHandler.gameObject.name + " has already been tested");
                        }
                    }

                    if(minDistanceBetweenConnected != -1)
                    {
                        if ((minDistanceBetweenConnected + distanceToConnection) < minDistanceBetweenConnection || minDistanceBetweenConnection == -1)
                        {
                            //Debug.Log("Better connection found with " + minDistanceBetweenConnected + distanceToConnection + " instead of " + minDistanceBetweenConnection);
                            minDistanceBetweenConnection = minDistanceBetweenConnected + distanceToConnection;

                            if(platform == currentPlatform)
                            {
                                //Debug.Log("Target Connection found ! + \"" + connection.gameObject.name + " is the chosen one ! Linked to " + tempCC.gameObject.name);
                                targetConnection = connection;
                                targetConnectedConnection = tempCC;
                            }
                        }
                    }
                    else
                    {
                        //Debug.Log("No CC linked to " + connection.gameObject.name + " lead to the player");
                    }
                }
                else
                {
                    //Debug.Log("Connection " + connection.gameObject.name + " has no connected connection");
                }
            }

            if(minDistanceBetweenConnection != -1)
            {
                return minDistanceBetweenConnection;
            }
            else
            {
                //Debug.Log(platform.gameObject.name + " has no connection leading to the player");
            }
        }
        return 0;
    }

    private void UpdatePath()
    {
        if (timeBeforeNextPathUpdate <= 0)
        {
            timeBeforeNextPathUpdate = pathUpdatingFrequency;

            if (useAStar)
            {
                CalculatePath();

                if (path != null)
                {
                    if (currentWaypoint >= path.vectorPath.Count)
                    {
                        pathEndReached = true;
                    }
                    else
                    {
                        pathEndReached = false;

                        while (Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance && path.vectorPath.Count > currentWaypoint + 1)
                        {
                            currentWaypoint++;
                        }

                        pathDirection = (path.vectorPath[currentWaypoint + waypointAhead] - transform.position).normalized;
                    }
                }
            }
            else if (IsOnGround())
            {
                FindGroundPathToTarget(GameData.playerManager.gameObject);
            }
        }

        if (timeBeforeNextPathUpdate > 0)
        {
            timeBeforeNextPathUpdate -= Time.deltaTime;
        }

        if (pJumpCDRemaining > 0)
        {
            pJumpCDRemaining -= Time.deltaTime;
        }
    }

    public IEnumerator JumpToConnection(PlatformConnection endConnection)
    {
        Vector2 jumpDifference = endConnection.transform.position - feetPos.position;
        SetEffect(Effect.NoControl, connectionJumpTime, false);
        isAffectedByGravity = false;
        collisionCollider.isTrigger = true;
        float timer = connectionJumpTime / slowEffectScale;
        pJumpCDRemaining = pathFindingJumpCooldown;

        while(timer > 0 && !Is(Effect.Stun) && !Is(Effect.NoGravity))
        {
            rb.velocity = jumpDifference / connectionJumpTime * slowEffectScale * Time.deltaTime * 50;

            yield return new WaitForEndOfFrame();

            timer -= Time.deltaTime;
        }

        if(timer <= 0)
        {
            transform.position = endConnection.transform.position - feetPos.localPosition;
            Propel(Vector2.zero, true, true);
            SetEffect(Effect.NoControl, 0.2f, false);
        }
        else
        {
            SetEffect(Effect.NoControl, 0, false);
        }

        collisionCollider.isTrigger = false;
        isAffectedByGravity = true;
    }

    private void AvoidOtherEnemies()
    {
        if(avoidEnemies)
        {
            ContactFilter2D enemyFilter = new ContactFilter2D();
            enemyFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
            List<Collider2D> closeEnemies = new List<Collider2D>();
            Physics2D.OverlapCircle(transform.position, avoidEffectRadius, enemyFilter, closeEnemies);
            foreach (Collider2D closeEnemy in closeEnemies)
            {
                if (closeEnemy.gameObject != gameObject)
                {
                    Vector2 directedForce = ((closeEnemy.transform.position - transform.position).normalized * avoidForce);
                    if (avoidDistanceInfluence != 0)
                    {
                        directedForce /= avoidDistanceInfluence * Vector2.Distance(transform.position, closeEnemy.transform.position);
                    }

                    if (directedForce.magnitude < minimalAvoidForce)
                    {
                        directedForce = directedForce.normalized * minimalAvoidForce;
                    }
                    else if (directedForce.magnitude > maximalAvoidForce)
                    {
                        directedForce = directedForce.normalized * maximalAvoidForce;
                    }

                    closeEnemy.GetComponent<EnemyHandler>().Propel(directedForce * Time.fixedDeltaTime, false, false);
                }
            }
        }
    }


    #endregion

    #region Combat


    public abstract void UpdateMovement();

    public void TakeDamage(int damage, Vector2 knockBack)
    {
        provoked = true;
        if (!isInvulnerable)
        {
            currentHealth -= damage;
            SetEffect(Effect.NoControl, invulnerableTime, false);
            StartCoroutine(Hurt());
            if (currentHealth <= 0)
            {
                StartCoroutine(DeathAnimation());
            }
        }
        Propel(knockBack, true, true);
    }

    public abstract bool TestCounter();

    public IEnumerator Hurt()
    {
        isInvulnerable = true;
        GetComponentInChildren<SpriteRenderer>().color = hurtColor;
        yield return new WaitForSeconds(invulnerableTime);
        GetComponentInChildren<SpriteRenderer>().color = baseColor;
        isInvulnerable = false;
    }

    public IEnumerator DeathAnimation()
    {
        isDead = true;
        SetEffect(Effect.NoControl, 50.0f, false);
        // death Animation
        yield return new WaitForSeconds(deathAnimationTime);
        Destroy(gameObject);
    }



    #endregion

    #region Test

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Hook") && !GameData.playerGrapplingHandler.isTracting && !isInvulnerable && currentHealth > 0 && !GameData.playerGrapplingHandler.isHooked && !isDead)
        {
            GameData.playerGrapplingHandler.AttachHook(gameObject);
        }
    }

    private bool IsOnGround()
    {
        bool isGrounded = false;
        if (Physics2D.OverlapBox(feetPos.position, new Vector2(groundCheckWidth, groundCheckThickness), 0.0f, groundMask) != null)
        {
            isGrounded = true;
        }

        return isGrounded;
    }

    public bool IsNearAnEdge(float distance)
    {
        int direction = -1;
        if (facingRight)
        {
            direction = 1;
        }
        return !Physics2D.OverlapCircle(new Vector2(feetPos.position.x + direction * distance, feetPos.position.y - 0.5f), 0.2f ,groundMask);
    }

    public bool PlayerInSight()
    {
        bool playerAhead = false;
        float startAngle = transform.rotation.eulerAngles.z + viewAngle - viewAngleWidth / 2;
        float subAngle = viewAngleWidth / raycastNumber;
        if(!facingRight)
        {
            startAngle += 180;
        }

        for(int i = 0; i < raycastNumber + 1; i++)
        {
            float angledDirection = (startAngle + i * subAngle) * Mathf.Deg2Rad;

            Vector2 direction = new Vector2(Mathf.Cos(angledDirection), Mathf.Sin(angledDirection));
            if(showFieldOfView)
            {
                Debug.DrawRay(transform.position, direction * viewRange, Color.white);
            }
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, viewRange, LayerMask.GetMask("Player", "Ground"));
            if (hit && hit.collider.CompareTag("Player"))
            {
                playerAhead = true;
            }
        }

        return playerAhead;
    }

    #endregion

    #region Effects

    private void UpdateGravity()
    {
        if (!Is(Effect.NoGravity) && !rb.freezeRotation)
        {
            rb.SetRotation(0);
            rb.freezeRotation = true;
            isAffectedByGravity = true;
            rb.sharedMaterial = basicMaterial;
        }
        else if(Is(Effect.NoGravity) && rb.freezeRotation)
        {
            rb.freezeRotation = false;
            rb.sharedMaterial = noGravityBouncyMaterial;
            Propel(Vector2.up * noGravityRiseUpForce, false, false);
            rb.angularVelocity = Random.Range(-noGravityRiseUpAngularRange, noGravityRiseUpAngularRange);
            rb.angularDrag = noGravityAngularDrag;
        }

        if(Is(Effect.NoGravity))
        {
            isAffectedByGravity = false;
            float dragForce = noGravityLinearDrag * Mathf.Pow(rb.velocity.magnitude, 2) / 2;
            rb.velocity -= rb.velocity * dragForce * Time.deltaTime;
        }

        if (isAffectedByGravity && gravityForce != 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y - gravityForce * Time.fixedDeltaTime);
        }
    }

    public void Propel(Vector2 directedForce, bool resetHorizontalVelocity, bool resetVerticalVelocity)
    {
        if(directedForce != null)
        {
            Vector2 newVelocity = Vector2.zero;

            if (resetVerticalVelocity)
            {
                newVelocity.y = directedForce.y;
            }
            else
            {
                newVelocity.y = rb.velocity.y + directedForce.y;
            }

            if (resetHorizontalVelocity)
            {
                newVelocity.x = directedForce.x;
            }
            else
            {
                newVelocity.x = rb.velocity.x + directedForce.x;
            }

            rb.velocity = new Vector2(newVelocity.x, newVelocity.y);
        }
        else
        {
            Debug.Log("Propel asked velocity for " + gameObject.name + " is not valid. Nan Nan");
        }
    }

    private void EffectUpdate()
    {
        for(int i = 0; i < currentEffects.Length; i++)
        {
            if(currentEffects[i] > 0)
            {
                currentEffects[i] -= Time.deltaTime;
            }
            else 
            {
                if (currentEffectFx[i] != null)
                {
                    Destroy(currentEffectFx[i]);
                }

                if(GameData.gameController.enemyEffects[i] == Effect.Slow)
                {
                    slowEffectScale = 1;
                }
            }
        }

        if(pauseAI)
        {
            SetEffect(Effect.NoControl, 1, false);
        }
    }

    public float SetEffect(Effect effect, float duration, bool isAdded)
    {
        if(effect == Effect.Slow)
        {
            Debug.Log("Slow effect added with a 50% scale. Use the function SetSlowEffect() to specify the scale");
            slowEffectScale = 0.5f;
        }

        if(effect == null)
        {
            Debug.Log("No corresponding effect found : No effect changed");
            Debug.Log(effect.effectName);
        }
        else
        {
            if (isAdded)
            {
                currentEffects[effect.index] += duration;
            }
            else if (currentEffects[effect.index] < duration)
            {
                if (duration != 0)
                {
                    currentEffects[effect.index] = duration;
                }
                else
                {
                    currentEffects[effect.index] = 0;
                }
            }
        }

        if(currentEffectFx[effect.index] == null && currentEffects[effect.index] > 0 && effect.effectFX != null)
        {
            currentEffectFx[effect.index] = Instantiate(effect.effectFX, transform);
        }

        return currentEffects[effect.index];
    }

    public void SetSlowEffect(float duration, bool isAdded, float slowScale)
    {
        slowEffectScale = slowScale;

        if (isAdded)
        {
            currentEffects[Effect.Slow.index] += duration;
        }
        else if (currentEffects[Effect.Slow.index] < duration)
        {
            if (duration != 0)
            {
                currentEffects[Effect.Slow.index] = duration;
            }
            else
            {
                currentEffects[Effect.Slow.index] = 0;
            }
        }
    }

    /// <summary>
    /// Return true if the enemy is affected by the specified effect
    /// </summary>
    /// <param name="effect"> The effect to test. Use preset effects from the Effect class like : Effect.Stun </param>
    /// <returns></returns>
    public bool Is(Effect effect)
    {
        bool isAffected = false;
        if(currentEffects[effect.index] > 0)
        {
            isAffected = true;
        }
        return isAffected;
    }

    private void UpdateMagnetism()
    {
        if(Is(Effect.Magnetism))
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Enemy"));
            List<Collider2D> colliders = new List<Collider2D>();
            Physics2D.OverlapCircle(transform.position, repulsionRange, filter, colliders);

            if (colliders.Count > 0)
            {
                foreach (Collider2D collider in colliders)
                {
                    EnemyHandler closeEnemy = collider.GetComponent<EnemyHandler>();
                    Vector2 direction = closeEnemy.transform.position - transform.position;
                    direction.Normalize();
                    closeEnemy.Propel(direction * repulsionForce, true, true);
                }
            }
        }
    }

    #endregion
}
