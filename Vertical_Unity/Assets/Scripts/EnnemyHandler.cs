using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public abstract class EnnemyHandler : MonoBehaviour
{
    [Header("Ennemy basic settings")]
    public new string name;
    public float maxHealth;
    public Color hurtColor;
    public float invulnerableTime;
    [Header("Ennemy pathfinding settings")]
    public float nextWaypointDistance;
    [Space]
    public float pathUpdatingFrequency;
    public float connectionJumpTime;
    [Header("Ennemy technical settings")]
    public Transform feetPos;
    public float groundCheckWidth;
    public float groundCheckThickness;
    public LayerMask walkableMask;
    public Vector2 colliderSize;

    public bool pauseAI;

    [HideInInspector] public float currentHealth;
    [HideInInspector] public bool isInvulnerable;
    [HideInInspector] public bool isStunned;
    [HideInInspector] public bool isInControl;
    [HideInInspector] public bool isAffectedByGravity;
    [HideInInspector] public bool isTouchingPlayer;
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

    private List<PlatformHandler> testedPlatforms = new List<PlatformHandler>();

    private Color baseColor;

    public void HandlerStart()
    {
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();

        baseColor = GetComponentInChildren<SpriteRenderer>().color;
        pathEndReached = false;
        isStunned = false;
        isInControl = true;
        isInvulnerable = false;
        isAffectedByGravity = true;
        isTouchingPlayer = false;
        currentHealth = maxHealth;

        timeBeforeNextGroundPathUpdate = 0;

        InvokeRepeating("CalculatePath", 0.0f, pathUpdatingFrequency);
    }

    public void HandlerUpdate()
    {
        UpdateGroundPath();

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

        isStunned = pauseAI;
    }

    public void HandlerFixedUpdate()
    {
        DetectPlayer();
    }

    void CalculatePath()
    {
        seeker.StartPath(transform.position, targetPathfindingPosition, OnPathComplete);
    }

    void OnPathComplete(Path p)
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
        Collider2D collider = Physics2D.OverlapCircle(feetPos.position, 2.0f, LayerMask.GetMask("Walkable"));
        if(collider != null)
        {
            Debug.Log("----Platform " + collider.gameObject.name + " found!");
            platform = collider.GetComponent<PlatformHandler>();  //DANGEROUS ------------------------------ !!
        }
        else
        {
            Debug.Log("No platform found");
        }

        return platform;
    }

    public bool FindGroundPathToTarget(GameObject target)
    {
        bool playerFound = false;

        Debug.Log("GROUND PATHFINDING STARTED");

        PlatformHandler platformFound = GetCurrentPlatform();
        if (platformFound != null)
        {
            currentPlatform = platformFound;
        }

        testedPlatforms.Clear();

        testedPlatforms.Add(currentPlatform);
        if(FindNextConnection(currentPlatform, target, feetPos.position))
        {
            playerFound = true;
            Debug.Log("Player accessible !");
        }

        return playerFound;
    }

    private bool FindNextConnection(PlatformHandler platform, GameObject target, Vector2 originConnectionPos)
    {
        Debug.Log("Testing player presence on " + platform.gameObject.name);
        if(platform.IsUnder(target))
        {
            Debug.Log("Player found on " + platform.gameObject.name + " !");
            return true;
        }
        else
        {
            Debug.Log("No player found. Now testing connections on " + platform.gameObject.name);
            foreach (PlatformConnection connection in platform.connections)
            {
                Debug.Log("Testing " + connection.gameObject.name);
                if(connection.connectedConnections.Count > 0)
                {
                    Debug.Log("Connection " + connection.gameObject.name + " has at least 1 connected connection");

                    foreach (PlatformConnection connectedConnection in connection.connectedConnections)
                    {
                        if(!testedPlatforms.Contains(connectedConnection.attachedPlatformHandler))
                        {
                            Debug.Log("The platform " + connectedConnection.attachedPlatformHandler.gameObject.name + " connected by the connection " + connectedConnection.gameObject.name + " has not been tested yet");

                            //float distanceToConnection = Vector2.Distance(originConnectionPos, connection.transform.position);

                            testedPlatforms.Add(connectedConnection.attachedPlatformHandler);
                            if(FindNextConnection(connectedConnection.attachedPlatformHandler, target, connectedConnection.transform.position))
                            {
                                targetConnection = connection;
                                targetConnectedConnection = connectedConnection;
                                return true;
                            }
                        }
                        else
                        {
                            Debug.Log("Skip > The platform " + connectedConnection.attachedPlatformHandler.gameObject.name + " connected by the connection " + connectedConnection.gameObject.name + " has already been tested");
                        }
                    }

                    Debug.Log("All connected connection on " + connection.gameObject.name + " has been tested with no results");
                }
                else
                {
                    Debug.Log("Connection " + connection.gameObject.name + " has no connected connection");
                }
            }
            Debug.Log("All connections on " + platform.gameObject.name + " has been tested with no results");
        }

        return false;
    }

    private void UpdateGroundPath()
    {
        if(timeBeforeNextGroundPathUpdate <= 0)
        {
            timeBeforeNextGroundPathUpdate = pathUpdatingFrequency;
            FindGroundPathToTarget(GameData.playerManager.gameObject);
        }

        if(timeBeforeNextGroundPathUpdate > 0)
        {
            timeBeforeNextGroundPathUpdate -= Time.deltaTime;
        }
    }

    public IEnumerator JumpToConnection(PlatformConnection endConnection)
    {
        Vector2 jumpDifference = endConnection.transform.position - feetPos.position;
        isInControl = false;
        isInvulnerable = true;
        isAffectedByGravity = false;
        float timer = connectionJumpTime;

        while(timer > 0 && !isStunned)
        {
            rb.velocity = jumpDifference / connectionJumpTime * Time.deltaTime * 50;

            yield return new WaitForEndOfFrame();

            timer -= Time.deltaTime;
        }

        if(!isStunned)
        {
            transform.position = endConnection.transform.position - feetPos.localPosition;
        }

        isAffectedByGravity = true;
        isInControl = true;
        isInvulnerable = false;
    }

    public abstract void UpdateMovement();

    public void TakeDamage(float damage, Vector2 knockBack, float stunTime)
    {
        if (!isInvulnerable)
        {
            currentHealth -= damage;
            StartCoroutine(Stun(stunTime));
            StartCoroutine(Hurt());
            if (currentHealth <= 0)
            {
                StartCoroutine(DeathAnimation());
            }
        }
        rb.velocity = knockBack;
    }

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

    private void DetectPlayer()
    {
        Collider2D collider = Physics2D.OverlapBox(transform.position, colliderSize, 0.0f, LayerMask.GetMask("Player"));
        if (collider != null)
        {
            isTouchingPlayer = true;
        }
        else
        {
            isTouchingPlayer = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Hook") && !GameData.playerGrapplingHandler.isTracting && !isInvulnerable && currentHealth > 0)
        {
            GameData.playerGrapplingHandler.AttachHook(gameObject);
        }
    }

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

    public bool IsOnGround()
    {
        bool isGrounded = false;
        if (Physics2D.OverlapBox(feetPos.position, new Vector2(groundCheckWidth, groundCheckThickness), 0.0f, walkableMask) != null)
        {
            isGrounded = true;
        }

        return isGrounded;
    }

    public IEnumerator Stun(float stunTime)
    {
        isStunned = true;
        yield return new WaitForSeconds(stunTime);
        isStunned = false;
    }

    public IEnumerator NoControl(float noControlTime)
    {
        isInControl = false;
        yield return new WaitForSeconds(noControlTime);
        isInControl = true;
    }
}
