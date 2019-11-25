using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHandler : MonoBehaviour
{
    public float knockBackForce;
    public float destroyTime;

    [HideInInspector] public Vector2 initialVelocity;

    void Start()
    {
        GetComponent<Rigidbody2D>().velocity = initialVelocity;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.CompareTag("Player"))
        {
            GameData.playerManager.TakeDamage(10.0f, (GameData.playerMovement.transform.position - transform.position).normalized * knockBackForce, 0.1f);
            StartCoroutine(DestroyProjectile());
        }
        else if(collider.CompareTag("Ground"))
        {
            StartCoroutine(DestroyProjectile());
        }
    }

    public IEnumerator DestroyProjectile()
    {
        if(destroyTime != 0)
        {
            yield return new WaitForSeconds(destroyTime);
        }
        //destroy animation
        Destroy(gameObject);
    }
}
