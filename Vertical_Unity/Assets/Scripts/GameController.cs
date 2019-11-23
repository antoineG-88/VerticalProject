using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public bool useMouseAndKeyboard;
    public List<Effect> enemyEffects;

    [HideInInspector] public float rightJoystickHorizontal;
    [HideInInspector] public float rightJoystickVertical;
    [HideInInspector] public float rightTriggerAxis;
    [HideInInspector] public bool rightTriggerDown;
    [HideInInspector] public bool rightBumper;

    private bool rtaFlag;
    private void Awake()
    {
        GameObject player = GameObject.FindWithTag("Player");
        GameData.Initialize(player.GetComponent<PlayerManager>(), player.GetComponent<PlayerMovement>(), player.GetComponent<PlayerGrapplingHandler>(), player.GetComponent<PlayerAttackManager>(), this);

        for (int i = 0; i < enemyEffects.Count; i++)
        {
            enemyEffects[i].index = i;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        InputUpdate();
    }

    private void InputUpdate()
    {
        rightJoystickHorizontal = Input.GetAxis("RJoystickH");
        rightJoystickVertical = -Input.GetAxis("RJoystickV");
        rightTriggerAxis = Input.GetAxis("RTAxis");
        rightBumper = Input.GetButton("RBButton");

        if (rtaFlag && !rightTriggerDown && rightTriggerAxis > 0.1f)
        {
            rightTriggerDown = true;
        }
        else if (rightTriggerDown)
        {
            rightTriggerDown = false;
            rtaFlag = false;
        }

        if (rightTriggerAxis <= 0.1f)
        {
            rtaFlag = true;
        }
    }
}
