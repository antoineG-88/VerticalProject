using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerFrappeSol", menuName = "PlayerAbilities/PowerFrappeSol")]
/// <summary>
/// The sample to follow to create a new Power
/// </summary>
public class PowerFrappeSol : Power
{
    public float frappevelocity;
    public float timePosInstantiate;
    private GameObject explosion;
    public GameObject prefabexplosion;
    public GameObject impactPrefab;
    public GameObject bulletPrefab;
    public Vector2 impactFxOffset;
    public float explosionRange;
    public float duration;
    public float size;
    private List<Vector2> effectPositions = new List<Vector2>();
    private List<GameObject> bullets = new List<GameObject>();

    public override IEnumerator Use()
    {
        effectPositions.Clear();
        float timer = 0;
        PlayerVisuals playerVisuals = GameData.playerManager.GetComponentInChildren<PlayerVisuals>();
        while(GameData.playerMovement.IsOnGround() == false)
        {
            GameData.playerMovement.Propel(Vector2.down * frappevelocity, true, true);
            GameData.playerMovement.inControl = false;

            playerVisuals.isSlaming = true;

            if(timer <= 0)
            {
                effectPositions.Add(GameData.playerMovement.transform.position);
                bullets.Add(Instantiate(bulletPrefab, GameData.playerMovement.transform.position, Quaternion.identity));
                timer = timePosInstantiate;
            }
            yield return new WaitForFixedUpdate();
            timer -= Time.fixedDeltaTime;
        }
        playerVisuals.isSlaming = false;

        Instantiate(impactPrefab, (Vector2)GameData.playerMovement.transform.position + impactFxOffset, Quaternion.identity);
        for (int i = 0; i < effectPositions.Count; i++)
        {
            explosion = Instantiate(prefabexplosion, effectPositions[i], Quaternion.identity);
            explosion.transform.localScale = new Vector2(size, size);
            Destroy(bullets[i]);
        }

        bullets.Clear();

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
