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
    [Header("Enemy pathfinding settings")]
    public bool useAStar;
    public float nextWaypointDistance;
    [Space]
    public float pathUpdatingFrequency;
    public float connectionJumpTime;
    public float pathfindingJumpWeight;
    public float pathFindingJumpCooldown;
    [Header("Enemy technical settings")]
    public Transform feetPos;
    public float groundCheckWidth;
    public float groundCheckThickness;
    public LayerMask groundMask;
    [Space]
    public bool pauseAI;
    public GameObject debugParticlePrefab;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public bool isInvulnerable;

    [HideInInspector] public float[] currentEffects;
    [HideInInspector] public GameObject[] currentEffectFx;
    [HideInInspector] public bool isAffectedByGravity;
    [HideInInspector] public Rigidbody2D rb;

    [HideInInspector] public Seeker seeker;
    [HideInInspector] public Path path;
    [HideInInspector] public int currentWaypoint;
    [HideInInspector] public bool pathEndReached;
    [HideInInspector] public Vector2 pathDirection;
    [HideInInspector] public Vector2 targetPathfindingPosition;

    [HideInInspector] public PlatformHandler currentPlatform;
    [HideInInspector] public PlatformConnection targetConnection;
    [HideInInspector] public PlatformConnection targetConnectedConnection;
    [HideInInspector] public float timeBeforeNextGroundPathUpdate;

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
        currentHealth = maxHealth;
        pJumpCDRemaining = 0;
        currentEffects = new float[GameData.gameController.enemyEffects.Count];
        currentEffectFx = new GameObject[GameData.gameController.enemyEffects.Count];
        for (int i = 0; i < currentEffects.Length; i++)
        {
            currentEffects[i] = 0;
        }

        timeBeforeNextGroundPathUpdate = 0;
    }

    public void HandlerUpdate()
    {
        UpdatePath();

        EffectUpdate();
    }

    public void HandlerFixedUpdate()
    {

    }

    #region Pathfinding


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
        if(useAStar)
        {
            CalculatePath();
        }

        if (timeBeforeNextGroundPathUpdate <= 0 && IsOnGround())
        {
            timeBeforeNextGroundPathUpdate = pathUpdatingFrequency;
            FindGroundPathToTarget(GameData.playerManager.gameObject);
        }

        if(timeBeforeNextGroundPathUpdate > 0)
        {
            timeBeforeNextGroundPathUpdate -= Time.deltaTime;
        }

        if(pJumpCDRemaining > 0)
        {
            pJumpCDRemaining -= Time.deltaTime;
        }



        if (path != null)
        {
            if (currentWaypoint >= path.vectorPath.Count)
            {
                pathEndReached = true;
            }
            else
            {
                pathEndReached = false;

                pathDirection = (path.vectorPath[currentWaypoint] - transform.position).normalized;

                if (Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance)
                {
                    currentWaypoint++;
                }
            }
        }
    }

    public IEnumerator JumpToConnection(PlatformConnection endConnection)
    {
        Vector2 jumpDifference = endConnection.transform.position - feetPos.position;
        SetEffect(Effect.NoControl, connectionJumpTime, false);
        isInvulnerable = true;
        isAffectedByGravity = false;
        float timer = connectionJumpTime;
        pJumpCDRemaining = pathFindingJumpCooldown;

        while(timer > 0 && !Is(Effect.Stun))
        {
            rb.velocity = jumpDifference / connectionJumpTime * Time.deltaTime * 50;

            yield return new WaitForEndOfFrame();

            timer -= Time.deltaTime;
        }

        if(!Is(Effect.Stun))
        {
            transform.position = endConnection.transform.position - feetPos.localPosition;
            Propel(Vector2.zero, true, true);
        }

        isAffectedByGravity = true;
        isInvulnerable = false;

        SetEffect(Effect.NoControl ,0.2f, false);
    }


    #endregion

    #region Combat


    public abstract void UpdateMovement();

    public void TakeDamage(int damage, Vector2 knockBack)
    {
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
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }



    #endregion

    #region Status test

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Hook") && !GameData.playerGrapplingHandler.isTracting && !isInvulnerable && currentHealth > 0)
        {
            GameData.playerGrapplingHandler.AttachHook(gameObject);
        }
    }

    public bool IsOnGround()
    {
        bool isGrounded = false;
        if (Physics2D.OverlapBox(feetPos.position, new Vector2(groundCheckWidth, groundCheckThickness), 0.0f, groundMask) != null)
        {
            isGrounded = true;
        }

        return isGrounded;
    }



    #endregion

    #region Effects


    public void Propel(Vector2 directedForce, bool resetHorizontalVelocity, bool resetVerticalVelocity)
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

    private void EffectUpdate()
    {
        for(int i = 0; i < currentEffects.Length; i++)
        {
            if(currentEffects[i] > 0)
            {
                currentEffects[i] -= Time.deltaTime;
            }
            else if(currentEffectFx[i] != null)
            {
                Destroy(currentEffectFx[i]);
            }
        }
    }

    public float SetEffect(Effect effect, float duration, bool isAdded)
    {
        if(effect.index < 0)
        {
            Debug.Log("No corresponding effect found : No effect changed");
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

    #endregion
}
