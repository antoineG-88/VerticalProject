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
    public float pathUpdatingFrequency;
    [Header("Ennemy technical settings")]
    public Transform feetPos;
    public float groundCheckWidth;
    public float groundCheckThickness;
    public LayerMask walkableMask;

    [HideInInspector] public float currentHealth;
    [HideInInspector] public bool isInvulnerable;
    [HideInInspector] public bool isStunned;
    [HideInInspector] public bool isInControl;
    [HideInInspector] public bool isTouchingPlayer;
    [HideInInspector] public Rigidbody2D rb;

    [HideInInspector] public Seeker seeker;
    [HideInInspector] public Path path;
    [HideInInspector] public int currentWaypoint;
    [HideInInspector] public bool pathEndReached;
    [HideInInspector] public Vector2 pathDirection;

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
        isTouchingPlayer = false;
        currentHealth = maxHealth;

        InvokeRepeating("CalculatePath", 0.0f, pathUpdatingFrequency);
    }

    public void HandlerUpdate()
    {
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

    void CalculatePath()
    {
        seeker.StartPath(transform.position, GameData.playerAttackManager.transform.position, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if(!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    public abstract void UpdateMovement();

    public void TakeDamage(float damage, Vector2 knockBack, float stunTime)
    {
        if (!isInvulnerable)
        {
            currentHealth -= damage;
            isInvulnerable = true;
            isStunned = true;
            StartCoroutine(Hurt(stunTime));
            if (currentHealth <= 0)
            {
                StartCoroutine(DeathAnimation());
            }
        }
        rb.velocity = knockBack;
    }

    public IEnumerator Hurt(float stunTime)
    {
        GetComponentInChildren<SpriteRenderer>().color = hurtColor;
        yield return new WaitForSeconds(invulnerableTime);
        GetComponentInChildren<SpriteRenderer>().color = baseColor;
        isInvulnerable = false;
        yield return new WaitForSeconds(stunTime - invulnerableTime);
        isStunned = false;
    }

    public IEnumerator DeathAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Hook") && !GameData.playerGrapplingHandler.isTracting && !isInvulnerable && currentHealth > 0)
        {
            GameData.playerGrapplingHandler.AttachHook(gameObject);
        }
        else if (collider.CompareTag("Player"))
        {
            isTouchingPlayer = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            isTouchingPlayer = false;
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

    public IEnumerator Stun(float stunTime, float delay)
    {
        yield return new WaitForSeconds(delay);
        isStunned = true;
        yield return new WaitForSeconds(stunTime);
        isStunned = false;
    }
}
