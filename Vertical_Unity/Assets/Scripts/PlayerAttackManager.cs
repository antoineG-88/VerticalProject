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
}
