using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    public static PlayerManager playerManager;
    public static PlayerMovement playerMovement;
    public static PlayerGrapplingHandler playerGrapplingHandler;
    public static PlayerAttackManager playerAttackManager;
    public static PlayerVisuals playerVisuals;
    public static GameController gameController;
    public static LevelBuilder levelBuilder;
    public static LevelHandler levelHandler;
    public static CameraHandler cameraHandler;
    public static AudioSource playerSource;

    public static void Initialize(PlayerManager _playerManager, PlayerMovement _playerMovement, PlayerGrapplingHandler _playerGrapplingHandler, PlayerAttackManager _playerAttackManager, GameController _gameController, CameraHandler _cameraHandler, LevelBuilder _levelBuilder, LevelHandler _levelHandler)
    {
        playerManager = _playerManager;
        playerMovement = _playerMovement;
        playerGrapplingHandler = _playerGrapplingHandler;
        playerAttackManager = _playerAttackManager;
        gameController = _gameController;
        levelBuilder = _levelBuilder;
        levelHandler = _levelHandler;
        cameraHandler = _cameraHandler;
        playerVisuals = _playerManager.GetComponentInChildren<PlayerVisuals>();
        playerSource = playerMovement.GetComponent<AudioSource>();
    }

    public static void Initialize(PlayerManager _playerManager, PlayerMovement _playerMovement, PlayerGrapplingHandler _playerGrapplingHandler, PlayerAttackManager _playerAttackManager, GameController _gameController, CameraHandler _cameraHandler)
    {
        playerManager = _playerManager;
        playerMovement = _playerMovement;
        playerGrapplingHandler = _playerGrapplingHandler;
        playerAttackManager = _playerAttackManager;
        gameController = _gameController;
        cameraHandler = _cameraHandler;
        playerVisuals = _playerManager.GetComponentInChildren<PlayerVisuals>();
        playerSource = playerMovement.GetComponent<AudioSource>();
    }
}
