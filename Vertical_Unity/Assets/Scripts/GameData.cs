using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData 
{
    public static PlayerManager playerManager;
    public static PlayerMovement playerMovement;
    public static PlayerGrapplingHandler playerGrapplingHandler;
    public static PlayerAttackManager playerAttackManager;

    public static void Initialize(PlayerManager _playerManager, PlayerMovement _playerMovement, PlayerGrapplingHandler _playerGrapplingHandler, PlayerAttackManager _playerAttackManager)
    {
        playerManager = _playerManager;
        playerMovement = _playerMovement;
        playerGrapplingHandler = _playerGrapplingHandler;
        playerAttackManager = _playerAttackManager;
    }
}
