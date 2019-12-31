using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyOrb : MonoBehaviour
{
    public float grabDistance;
    public float gravityAmplitude;
    public float playerGravityScale;
    public float playerBaseGravity;
    public float initialVelocityForce;
    public float unpickableTime;
    public int energyHeld;
    public GameObject pickParticle;
    public float lifeSpan;

    [HideInInspector] public bool isPickable;
    private Rigidbody2D rb;
    private Vector2 playerRelativePos;
    private float unpickableTimeRemaining;
    private float lifeSpend;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Vector2 initialVelocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        initialVelocity.Normalize();
        float initialForce = Random.Range(2, initialVelocityForce + 1);
        rb.velocity = initialVelocity * initialForce;
        unpickableTimeRemaining = unpickableTime;
        isPickable = false;
        lifeSpend = 0;
    }

    private void FixedUpdate()
    {
        UpdatePositions();

        if(lifeSpend < lifeSpan)
        {
            lifeSpend += Time.fixedDeltaTime;
        }
        else
        {
            Instantiate(pickParticle, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void UpdatePositions()
    {
        if (isPickable)
        {
            playerRelativePos = GameData.playerMovement.transform.position - transform.position;
            if (playerRelativePos.magnitude < grabDistance)
            {
                rb.velocity += (playerRelativePos.normalized * playerGravityScale) / Mathf.Pow(playerRelativePos.magnitude - 2 >= 1 ? playerRelativePos.magnitude - 2 : 1, gravityAmplitude) + (playerRelativePos.normalized * playerBaseGravity);
            }
        }
        else
        {
            unpickableTimeRemaining -= Time.fixedDeltaTime;
            if(unpickableTimeRemaining <= 0)
            {
                isPickable = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && isPickable)
        {
            Consume();
        }
    }

    private void Consume()
    {
        GameData.playerManager.EarnEnergy(energyHeld);
        Instantiate(pickParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
