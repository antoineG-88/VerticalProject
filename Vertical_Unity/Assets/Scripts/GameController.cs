using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public AstarPath astarPath;
    public Text speedText;
    public Text debugText;
    public GameObject debubParticle;
    public List<Effect> enemyEffects;
    [HideInInspector] public InputManager input;

    private void Awake()
    {
        input = GetComponent<InputManager>();
        GameObject player = GameObject.FindWithTag("Player");
        GameData.Initialize(player.GetComponent<PlayerManager>(), player.GetComponent<PlayerMovement>(), player.GetComponent<PlayerGrapplingHandler>(), player.GetComponent<PlayerAttackManager>(), this);

        for (int i = 0; i < enemyEffects.Count; i++)
        {
            enemyEffects[i].index = i;
        }
    }

    private void Start()
    {
        astarPath.graphs[0].Scan();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
