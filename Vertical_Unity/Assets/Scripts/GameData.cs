﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    public static PlayerManager playerManager;
    public static PlayerMovement playerMovement;
    public static PlayerGrapplingHandler playerGrapplingHandler;
    public static PlayerAttackManager playerAttackManager;
    public static GameController gameController;
    public static LevelBuilder levelBuilder;
    public static LevelHandler levelHandler;

    public static void Initialize(PlayerManager _playerManager, PlayerMovement _playerMovement, PlayerGrapplingHandler _playerGrapplingHandler, PlayerAttackManager _playerAttackManager, GameController _gameController, LevelBuilder _levelBuilder, LevelHandler _levelHandler)
    {
        playerManager = _playerManager;
        playerMovement = _playerMovement;
        playerGrapplingHandler = _playerGrapplingHandler;
        playerAttackManager = _playerAttackManager;
        gameController = _gameController;
        levelBuilder = _levelBuilder;
        levelHandler = _levelHandler;
    }

    public static void Initialize(PlayerManager _playerManager, PlayerMovement _playerMovement, PlayerGrapplingHandler _playerGrapplingHandler, PlayerAttackManager _playerAttackManager, GameController _gameController)
    {
        playerManager = _playerManager;
        playerMovement = _playerMovement;
        playerGrapplingHandler = _playerGrapplingHandler;
        playerAttackManager = _playerAttackManager;
        gameController = _gameController;
    }
}
