using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Initial Player Info")]
    public float maxhealth;
    [Header("General debug settings")]
    public Color stunColor;
    public bool isVulnerable;

    [HideInInspector] public bool isDodging;
    private float currentHealth;
    private Rigidbody2D rb;
    private Color baseColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxhealth;
        baseColor = GetComponentInChildren<SpriteRenderer>().color;
        isDodging = false;
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

            if (currentHealth <= 0)
            {
                GetComponent<SpriteRenderer>().enabled = false;
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
        yield return new WaitForSeconds(stunTime);
        GetComponentInChildren<SpriteRenderer>().color = baseColor;
        GameData.playerMovement.inControl = true;
        GameData.playerGrapplingHandler.canShoot = true;
    }
}
