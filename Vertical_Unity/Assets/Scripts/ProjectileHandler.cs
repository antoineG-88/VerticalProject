using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHandler : MonoBehaviour
{
    public GameObject desintegrationPrefab;

    [HideInInspector] public int damage;
    [HideInInspector] public float knockBackForce;
    [HideInInspector] public float destroyTime;
    [HideInInspector] public float lifeTime;
    [HideInInspector] public Vector2 initialVelocity;

    void Start()
    {
        GetComponent<Rigidbody2D>().velocity = initialVelocity;

        if (lifeTime > 0)
        {
            StartCoroutine(Despawn());
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.CompareTag("Player"))
        {
            GameData.playerManager.TakeDamage(damage, (GameData.playerMovement.transform.position - transform.position).normalized * knockBackForce, 0.1f);
            if(!GameData.playerMovement.isDashing)
            {
                StartCoroutine(DestroyProjectile());
            }
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
        Instantiate(desintegrationPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}
