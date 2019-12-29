using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Initial Player Info")]
    public int maxhealth;
    public int baseEnergy;
    public float invulnerableTime;
    [Header("General debug settings")]
    public Color stunColor;
    public bool vulnerable;

    [HideInInspector] public bool isVulnerable;
    [HideInInspector] public bool isDodging;
    [HideInInspector] public bool isStunned;
    private int currentHealth;  // à récupérer pour l'interface
    private int currentEnergy;
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
        currentEnergy = baseEnergy;
    }

    private void Update()
    {
        if (!GameData.gameController.pause)
        {
            UpdatePlayerStatus();

            if (Input.GetKeyDown(KeyCode.M))
            {
                PlayerData.playerHealth = currentHealth;
                Debug.Log("CurrentHealth : " + currentHealth + " saved in PlayerData");
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("PlayerData health : " + PlayerData.playerHealth);
            }
        }
    }

    public bool TakeDamage(int damage, Vector2 knockBack, float stunTime)
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
                StartCoroutine(Die());
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

    private void UpdatePlayerStatus()
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

    private IEnumerator Die()
    {
        GetComponentInChildren<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        rb.simulated = false;
        GameData.playerMovement.inControl = false;
        GameData.playerGrapplingHandler.canShoot = false;

        yield return new WaitForSeconds(0);
    }
}
