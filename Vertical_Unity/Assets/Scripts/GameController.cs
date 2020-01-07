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
    public GameObject loadingPanel;
    public float closingTime;
    [Header("General settings")]
    public Text speedText;
    public Text debugText;
    public GameObject debubParticle;

    [HideInInspector] public InputManager input;
    [HideInInspector] public bool pause;
    [HideInInspector] public bool takePlayerInput;
    [HideInInspector] public PostProcessHandler postProcessHandler;

    private void Awake()
    {
        postProcessHandler = GameObject.Find("PostProcess").GetComponent<PostProcessHandler>();
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
            RestartLevel();
        }
    }

    public void LoadNextLevel()
    {
        SavePlayerData();
        SceneManager.LoadScene(nextSceneIndex);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SavePlayerData()
    {
        PlayerData.playerHealth = GameData.playerManager.currentHealth;
        PlayerData.playerEnergy = GameData.playerManager.currentEnergy;
        PlayerData.playerKick = GameData.playerAttackManager.currentKick;
        PlayerData.playerPower = GameData.playerAttackManager.currentPower;
    }

    public IEnumerator CloseLoading()
    {
        float timer = closingTime;
        Image panelImage = loadingPanel.GetComponent<Image>();
        loadingPanel.transform.GetChild(0).gameObject.SetActive(false);
        loadingPanel.transform.GetChild(1).gameObject.SetActive(false);
        float reduce = 1 / closingTime * Time.fixedDeltaTime;
        while (timer > 0)
        {
            panelImage.color = new Color(0, 0, 0, panelImage.color.a - reduce);

            timer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        panelImage.color = new Color(0, 0, 0, 0);
        loadingPanel.SetActive(false);
    }

    public void OpenLoading()
    {
        loadingPanel.SetActive(true);

        loadingPanel.transform.GetChild(0).gameObject.SetActive(true);
        loadingPanel.transform.GetChild(1).gameObject.SetActive(true);
        loadingPanel.GetComponent<Image>().color = new Color(0, 0, 0, 1);
    }

    public void SwitchGodMod()
    {
        GameData.playerManager.vulnerable = !GameData.playerManager.vulnerable;
    }

}
