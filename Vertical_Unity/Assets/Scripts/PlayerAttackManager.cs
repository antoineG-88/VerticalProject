using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    public Kick currentKick;
    [Header("Collide settings")]
    public Vector2 collideSize;

    private void Update()
    {
        if(Input.GetButtonDown("XButton") && GameData.playerGrapplingHandler.isTracting)
        {
            Collider2D testedCollider = currentKick.HitTest();
            if (testedCollider != null && GameData.playerGrapplingHandler.attachedObject == testedCollider.gameObject)
            {
                TriggerKick(testedCollider.GetComponent<EnnemyHandler>());
            }
        }
    }

    private void FixedUpdate()
    {
        Collider2D enemyCollider = Physics2D.OverlapBox(transform.position, collideSize, 0.0f, LayerMask.GetMask("Ennemy"));
        if (enemyCollider != null && GameData.playerGrapplingHandler.isTracting && GameData.playerGrapplingHandler.attachedObject == enemyCollider.gameObject)
        {
            TriggerKick(enemyCollider.GetComponent<EnnemyHandler>());
        }
    }

    /// <summary>
    /// Replace the equipped Kick with a new one, return the kick replaced
    /// </summary>
    /// <param name="newKick">The kick that will be replacing the old one</param>
    /// <returns></returns>
    public Kick ReplaceCurrentKick(Kick newKick)
    {
        Kick previousKick = currentKick;
        currentKick = newKick;
        return previousKick;
    }

    public void TriggerKick(EnnemyHandler ennemy)
    {
        currentKick.Use(ennemy);
    }
}
