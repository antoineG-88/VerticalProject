using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewKickSample", menuName = "PlayerAbilities/KickSample")]
/// <summary>
/// The sample to follow to create a new Kick
/// </summary>
public class KickSample: Kick
{
    public float damagePerHit;
    public float criticalDamage;

    public float ennemyKnockBackForce;
    public float playerKnockBackForce;
    public Vector2 addedEnnemyKnockBack;
    public Vector2 addedPlayerKnockBack;

    private int repetition = 0;
    private float damageToDeal;
    public override void Use(EnnemyHandler ennemy, PlayerMovement playerMovement, PlayerGrapplingHandler playerGrapplingHandler)
    {
        damageToDeal = damagePerHit;
        if (repetition <= 2)
        {
            repetition++;
        }
        if(repetition > 2 && playerGrapplingHandler.attachedObject == null)
        {
            repetition = 0;
            damageToDeal = criticalDamage;
        }
        Vector2 kickDirection = new Vector2(ennemy.transform.position.x - playerMovement.transform.position.x, ennemy.transform.position.y - playerMovement.transform.position.y).normalized;
        playerMovement.DisableControl(0.3f, false);
        playerMovement.Propel(-kickDirection * playerKnockBackForce + addedPlayerKnockBack, true, true);
        ennemy.TakeDamage(damageToDeal, kickDirection * ennemyKnockBackForce + addedEnnemyKnockBack, 0.3f);
        playerGrapplingHandler.ReleaseHook();
    }
}
