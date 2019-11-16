using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewKickSample", menuName = "PlayerAbilities/KickSample")]
/// <summary>
/// The sample to follow to create a new Kick
/// </summary>
public class KickSample: Kick
{
    public float stunTime;

    public float enemyKnockBackForce;
    public Vector2 addedEnemyKnockBack;
    public override IEnumerator Use(GameObject player, Quaternion kickRotation)
    {
        GameObject kickEffect = Instantiate(kickVisualEffect, player.transform);
        kickEffect.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.right, GameData.playerGrapplingHandler.tractionDirection));

        yield return new WaitForSeconds(timeBeforeKick);

        GameData.playerAttackManager.Hit();
    }

    public override void DealDamageToEnemy(EnemyHandler enemy)
    {
        Vector2 kickDirection = new Vector2(enemy.transform.position.x - GameData.playerMovement.transform.position.x, enemy.transform.position.y - GameData.playerMovement.transform.position.y).normalized;
        GameData.playerMovement.DisableControl(0.3f, false);
        enemy.TakeDamage(1, kickDirection * enemyKnockBackForce + addedEnemyKnockBack, stunTime);
        GameData.playerGrapplingHandler.ReleaseHook();
    }
}
