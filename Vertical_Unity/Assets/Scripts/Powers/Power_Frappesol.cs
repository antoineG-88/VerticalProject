using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerFrappeSol", menuName = "PlayerAbilities/PowerFrappeSol")]
/// <summary>
/// The sample to follow to create a new Power
/// </summary>
public class PowerFrappeSol : Power
{
    public float range;
    public float stunTime;
    public float frappevelocity;
    public float timePosInstantiate;
    private GameObject explosion;
    public GameObject prefabexplosion;
    public float explosionRange;
    public float duration;

    private List<Vector2> effectPositions = new List<Vector2>();

    public override IEnumerator Use()
    {
        effectPositions.Clear();
        float timer = 0;
        while(GameData.playerMovement.IsOnGround() == false)
        {
            GameData.playerMovement.Propel(Vector2.down* frappevelocity, true, true);
            GameData.playerMovement.inControl = false;

            if(timer <= 0)
            {
                effectPositions.Add(GameData.playerMovement.transform.position);
                timer = timePosInstantiate;
            }
            yield return new WaitForFixedUpdate();
            timer -= Time.fixedDeltaTime;
            Debug.Log(timer);
        }
        for (int i = 0; i < effectPositions.Count; i++)
        {
            explosion = Instantiate(prefabexplosion, effectPositions[i], Quaternion.identity);
        }

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Enemy"));
        List<Collider2D> enemycolliders = new List<Collider2D>();
        for(int i = 0; i < effectPositions.Count; i++)
        {
            Physics2D.OverlapCircle(effectPositions[i], explosionRange, filter, enemycolliders);

            if (enemycolliders.Count > 0)
            {
                foreach (Collider2D collider in enemycolliders)
                {
                    EnemyHandler closeEnemy = collider.GetComponent<EnemyHandler>();
                    closeEnemy.SetEffect(Effect.Magnetism, duration, false);
                }
            }
        }

        GameData.playerMovement.inControl = true;

        yield return new WaitForFixedUpdate();
        yield return new WaitForSeconds(0.2f);
    }
}
