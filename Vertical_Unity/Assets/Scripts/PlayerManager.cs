using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Initial Player Info")]
    public float maxhealth;
    public float invulnerableTime;
    [Header("General debug settings")]
    public Color stunColor;
    public bool vulnerable;

    [HideInInspector] public bool isVulnerable;
    [HideInInspector] public bool isDodging;
    [HideInInspector] public bool isStunned;
    private float currentHealth;
    private Rigidbody2D rb;
    private Color baseColor;
    private float invulnerableTimeRemaining;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxhealth;
        baseColor = GetComponentInChildren<SpriteRenderer>().color;
        isDodging = false;
        isStunned = false;
        invulnerableTimeRemaining = 0;
    }

    private void Update()
    {
        if (invulnerableTimeRemaining > 0)
        {
            invulnerableTimeRemaining -= Time.deltaTime;
        }
        else
        {
            isVulnerable = true;
        }

        isVulnerable = vulnerable ? isVulnerable : false;
    }

    public bool TakeDamage(float damage, Vector2 knockBack, float stunTime)
    {
        bool tookDamage = false;
        if(isVulnerable && !isDodging)
        {
            tookDamage = true;
            currentHealth -= damage;
            GameData.playerMovement.Propel(knockBack, true, true);
            GameData.playerGrapplingHandler.ReleaseHook();
            StartCoroutine(Stun(stunTime));
            invulnerableTimeRemaining = invulnerableTime;
            isVulnerable = false;
            if (currentHealth <= 0)
            {
                GetComponentInChildren<SpriteRenderer>().enabled = false;
                GetComponent<Collider2D>().enabled = false;
                rb.simulated = false;
                GameData.playerMovement.inControl = false;
                GameData.playerGrapplingHandler.canShoot = false;
            }
        }

        return tookDamage;
    }

    public IEnumerator Stun(float stunTime)
    {
        GetComponentInChildren<SpriteRenderer>().color = stunColor;
        GameData.playerMovement.inControl = false;
        GameData.playerGrapplingHandler.canShoot = false;
        isStunned = true;
        yield return new WaitForSeconds(stunTime);
        GetComponentInChildren<SpriteRenderer>().color = baseColor;
        isStunned = false;
        GameData.playerMovement.inControl = true;
        GameData.playerGrapplingHandler.canShoot = true;
    }
}
