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
    [Header("General settings")]
    public Text speedText;
    public Text debugText;
    public GameObject debubParticle;
    public List<Effect> enemyEffects;

    [HideInInspector] public InputManager input;
    [HideInInspector] public bool pause;

    private void Awake()
    {
        pause = false;
        input = GetComponent<InputManager>();
        GameObject player = GameObject.FindWithTag("Player");
        GameObject level = GameObject.Find("Level");
        if (level != null)
        {
            GameData.Initialize(player.GetComponent<PlayerManager>(), player.GetComponent<PlayerMovement>(), player.GetComponent<PlayerGrapplingHandler>(), player.GetComponent<PlayerAttackManager>(), this, level.GetComponent<LevelBuilder>(), level.GetComponent<LevelHandler>());
        }
        else
        {
            GameData.Initialize(player.GetComponent<PlayerManager>(), player.GetComponent<PlayerMovement>(), player.GetComponent<PlayerGrapplingHandler>(), player.GetComponent<PlayerAttackManager>(), this);
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
        SceneManager.LoadScene(nextSceneIndex);
    }
}
