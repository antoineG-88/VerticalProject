using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnnemyHandler : MonoBehaviour
{
    [Header("Ennemy basic settings")]
    public new string name;
    public float maxHealth;
    public Color hurtColor;
    public float invulnerableTime;

    [HideInInspector] public float currentHealth;
    [HideInInspector] public bool isInvulnerable;
    [HideInInspector] public bool isStunned;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public PlayerGrapplingHandler playerGrapplingHandler;
    [HideInInspector] public PlayerAttackManager playerAttackManager;

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
        GetComponent<SpriteRenderer>().color = hurtColor;
        yield return new WaitForSeconds(invulnerableTime);
        GetComponent<SpriteRenderer>().color = Color.white;
        isInvulnerable = false;
        yield return new WaitForSeconds(stunTime - invulnerableTime);
        isStunned = false;
    }

    public IEnumerator DeathAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Hook") && !playerGrapplingHandler.isTracting && !isInvulnerable && currentHealth > 0)
        {
            playerGrapplingHandler.AttachHook(gameObject);
        }
        else if (collider.CompareTag("Player") && playerGrapplingHandler.isTracting && gameObject == playerGrapplingHandler.attachedObject)
        {
            playerAttackManager.TriggerKick(this);
        }
    }
}
