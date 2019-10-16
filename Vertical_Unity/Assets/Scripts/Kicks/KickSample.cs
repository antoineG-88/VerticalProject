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
    public float stunTime;

    public float ennemyKnockBackForce;
    public Vector2 addedEnnemyKnockBack;

    private int repetition = 0;
    private float damageToDeal;
    public override void Use(EnnemyHandler ennemy)
    {
        damageToDeal = damagePerHit;
        if (repetition <= 2)
        {
            repetition++;
        }
        if(repetition > 2 && GameData.playerGrapplingHandler.attachedObject == null)
        {
            repetition = 0;
            damageToDeal = criticalDamage;
        }
        Vector2 kickDirection = new Vector2(ennemy.transform.position.x - GameData.playerMovement.transform.position.x, ennemy.transform.position.y - GameData.playerMovement.transform.position.y).normalized;
        GameData.playerMovement.DisableControl(0.3f, false);
        ennemy.TakeDamage(damageToDeal, kickDirection * ennemyKnockBackForce + addedEnnemyKnockBack, stunTime);
        GameData.playerGrapplingHandler.ReleaseHook();

        Vector2 propelingDirection = -kickDirection;
        if(GameData.gameController.rightJoystickHorizontal != 0 || GameData.gameController.rightJoystickVertical != 0)
        {
            propelingDirection.x = GameData.gameController.rightJoystickHorizontal;
            propelingDirection.y = GameData.gameController.rightJoystickVertical;
            propelingDirection.Normalize();
            Debug.Log(propelingDirection);
        }

        GameData.playerManager.Stun(0.1f);
        GameData.playerMovement.Propel(propelingDirection * propelingForce, true, true);
    }
}
