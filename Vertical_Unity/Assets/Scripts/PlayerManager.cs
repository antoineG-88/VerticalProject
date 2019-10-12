using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Initial Player Info")]
    public float maxhealth;
    [Header("General debug settings")]
    public Color stunColor;

    private float currentHealth;
    private PlayerMovement playerMovement;
    private PlayerGrapplingHandler playerGrapplingHandler;
    private Rigidbody2D rb;
    private Color baseColor;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerGrapplingHandler = GetComponent<PlayerGrapplingHandler>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxhealth;
        baseColor = GetComponent<SpriteRenderer>().color;
    }

    public void TakeDamage(float damage, Vector2 knockBack, float stunTime)
    {
        currentHealth -= damage;
        playerMovement.Propel(knockBack, true, true);
        StartCoroutine(Stun(stunTime));
    }

    private IEnumerator Stun(float stunTime)
    {
        GetComponent<SpriteRenderer>().color = stunColor;
        playerMovement.inControl = false;
        playerGrapplingHandler.canShoot = false;
        yield return new WaitForSeconds(stunTime);
        GetComponent<SpriteRenderer>().color = baseColor;
        playerMovement.inControl = true;
        playerGrapplingHandler.canShoot = true;
    }
}
