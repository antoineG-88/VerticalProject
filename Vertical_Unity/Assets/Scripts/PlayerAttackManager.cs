using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    public Kick currentKick;

    private PlayerGrapplingHandler playerGrapplingHandler;
    private PlayerMovement playerMovement;

    void Start()
    {
        playerGrapplingHandler = GetComponent<PlayerGrapplingHandler>();
        playerMovement = GetComponent<PlayerMovement>();
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
        currentKick.Use(ennemy, playerMovement, playerGrapplingHandler);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.GetMask("ennemy") && playerGrapplingHandler.isTracting && playerGrapplingHandler.attachedObject == collider.gameObject)
        {
            EnnemyHandler ennemy = collider.GetComponent<EnnemyHandler>();
            TriggerKick(ennemy);
        }
    }
}
