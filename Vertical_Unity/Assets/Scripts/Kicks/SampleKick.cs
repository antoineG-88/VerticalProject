using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The sample to follow to create a new Kick
/// </summary>
public class SampleKick : Kick
{
    public float ennemyKnockBackForce;
    public float playerKnockBackForce;
    public override void TriggerAttack(EnnemyHandler ennemy, PlayerMovement playerMovement, PlayerGrapplingHandler playerGrapplingHandler)
    {
        Vector2 ennemyKnockBack = new Vector2(ennemy.transform.position.x - playerMovement.transform.position.x, ennemy.transform.position.y - playerMovement.transform.position.y).normalized * ennemyKnockBackForce;
        playerMovement.DisableControl(0.3f, false);
        playerMovement.SetVelocity(new Vector2(-knockbackFinal.x, -knockbackFinal.y + knockbackUp));
        knockbackFinal.y += knockbackUp;
        ennemy.TakeDamage(10.0f, knockbackFinal, 0.3f);
        playerGrapplingHandler.ReleaseHook();
    }
}
