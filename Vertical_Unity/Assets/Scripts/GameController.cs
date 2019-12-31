using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("References")]
    public int nextSceneIndex;
    public int mainMenuSceneIndex = 0;
    public List<Effect> enemyEffects;
    public List<Power> allPowers;
    [Header("General settings")]
    public Text speedText;
    public Text debugText;
    public GameObject debubParticle;

    [HideInInspector] public InputManager input;
    [HideInInspector] public bool pause;
    [HideInInspector] public bool takePlayerInput;

    private void Awake()
    {
        takePlayerInput = true;
        pause = false;
        input = GetComponent<InputManager>();
        GameObject player = GameObject.FindWithTag("Player");
        GameObject level = GameObject.Find("Level");
        GameObject mainCamera = GameObject.FindWithTag("MainCamera");
        if (level != null)
        {
            GameData.Initialize(player.GetComponent<PlayerManager>(), player.GetComponent<PlayerMovement>(), player.GetComponent<PlayerGrapplingHandler>(), player.GetComponent<PlayerAttackManager>(), this, mainCamera.GetComponent<CameraHandler>(), level.GetComponent<LevelBuilder>(), level.GetComponent<LevelHandler>());
        }
        else
        {
            GameData.Initialize(player.GetComponent<PlayerManager>(), player.GetComponent<PlayerMovement>(), player.GetComponent<PlayerGrapplingHandler>(), player.GetComponent<PlayerAttackManager>(), this, mainCamera.GetComponent<CameraHandler>());
        }

        for (int i = 0; i < enemyEffects.Count; i++)
        {
            enemyEffects[i].index = i;
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.T))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void LoadNextLevel()
    {
        SavePlayerData();
        SceneManager.LoadScene(nextSceneIndex);
    }

    public void SavePlayerData()
    {
        PlayerData.playerHealth = GameData.playerManager.currentHealth;
        PlayerData.playerEnergy = GameData.playerManager.currentEnergy;
        PlayerData.playerKick = GameData.playerAttackManager.currentKick;
        PlayerData.playerPower = GameData.playerAttackManager.currentPower;
    }
}
