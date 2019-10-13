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

    private float currentHealth;
    private Rigidbody2D rb;
    private Color baseColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxhealth;
        baseColor = GetComponent<SpriteRenderer>().color;
    }

    public void TakeDamage(float damage, Vector2 knockBack, float stunTime)
    {
        if(isVulnerable)
        {
            currentHealth -= damage;
            GameData.playerMovement.Propel(knockBack, true, true);
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
    }

    private IEnumerator Stun(float stunTime)
    {
        GetComponent<SpriteRenderer>().color = stunColor;
        GameData.playerMovement.inControl = false;
        GameData.playerGrapplingHandler.canShoot = false;
        yield return new WaitForSeconds(stunTime);
        GetComponent<SpriteRenderer>().color = baseColor;
        GameData.playerMovement.inControl = true;
        GameData.playerGrapplingHandler.canShoot = true;
    }
}
