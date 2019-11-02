using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [HideInInspector] public float rightJoystickHorizontal;
    [HideInInspector] public float rightJoystickVertical;
    [HideInInspector] public float rightTriggerAxis;

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        GameData.Initialize(player.GetComponent<PlayerManager>(), player.GetComponent<PlayerMovement>(), player.GetComponent<PlayerGrapplingHandler>(), player.GetComponent<PlayerAttackManager>(), this);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        rightJoystickHorizontal = Input.GetAxis("RJoystickH");
        rightJoystickVertical = -Input.GetAxis("RJoystickV");
        rightTriggerAxis = Input.GetAxis("RTAxis");
    }
}
