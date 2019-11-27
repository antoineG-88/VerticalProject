using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewKickSample", menuName = "PlayerAbilities/KickSample")]
/// <summary>
/// The sample to follow to create a new Kick
/// </summary>
public class KickSample: Kick
{
    [Header("Perfect Timing settings")]
    public float ptStunTime;
    public float ptStunRange;
    public GameObject AOEStunFx;

    public float enemyKnockBackForce;
    public Vector2 addedEnemyKnockBack;
    public override IEnumerator Use(GameObject player, Quaternion kickRotation)
    {
        GameObject kickEffect = Instantiate(kickVisualEffect, player.transform.position, player.transform.rotation);
        kickEffect.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.right, GameData.playerGrapplingHandler.tractionDirection));

        if(timeBeforeKick > 0)
        {
            yield return new WaitForSeconds(timeBeforeKick);
        }

        GameData.playerAttackManager.Hit();
    }

    public override void DealDamageToEnemy(EnemyHandler enemy)
    {
        Vector2 kickDirection = new Vector2(enemy.transform.position.x - GameData.playerMovement.transform.position.x, enemy.transform.position.y - GameData.playerMovement.transform.position.y).normalized;
        GameData.playerMovement.DisableControl(0.3f, false);
        enemy.TakeDamage(1, kickDirection * enemyKnockBackForce + addedEnemyKnockBack);
        GameData.playerGrapplingHandler.ReleaseHook();
    }

    public override void ApplyPerfectTimingEffect(EnemyHandler enemy)
    {
        Instantiate(AOEStunFx, enemy.transform.position, Quaternion.identity);
        List<Collider2D> colliders = new List<Collider2D>();
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
        Physics2D.OverlapCircle(enemy.transform.position, ptStunRange, contactFilter, colliders);
        foreach(Collider2D collider in colliders)
        {
            collider.GetComponent<EnemyHandler>().SetEffect(Effect.Stun, ptStunTime, true);
        }
    }
}
