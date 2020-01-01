using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public bool useMouseAim;
    public bool useMouseMovement;

    [HideInInspector] public float rightJoystickHorizontal;
    [HideInInspector] public float rightJoystickVertical;
    [HideInInspector] public float leftJoystickHorizontal;
    [HideInInspector] public float leftJoystickVertical;
    [HideInInspector] public float rightTriggerAxis;
    [HideInInspector] public bool rightTriggerDown;
    [HideInInspector] public float leftTriggerAxis;
    [HideInInspector] public bool leftTriggerDown;
    [HideInInspector] public bool rightBumper;
    [HideInInspector] public bool jump;

    private bool rtaFlag;
    private bool ltaFlag;

    void Update()
    {
        InputUpdate();
    }

    private void InputUpdate()
    {
        if (useMouseAim)
        {
            if (!useMouseMovement)
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 rightJoystickDirection = (mouseWorldPos - (Vector2)GameData.playerMovement.transform.position).normalized;
                rightJoystickHorizontal = rightJoystickDirection.x;
                rightJoystickVertical = rightJoystickDirection.y;
            }

            leftJoystickHorizontal = GameData.gameController.takePlayerInput ? Input.GetAxisRaw("Horizontal") : 0;
            leftJoystickVertical = GameData.gameController.takePlayerInput ? Input.GetAxisRaw("Vertical") : 0;

            rightTriggerAxis = Input.GetButton("Tract") && GameData.gameController.takePlayerInput ? 1 : 0;
            leftTriggerAxis = Input.GetButton("Power") && GameData.gameController.takePlayerInput ? 1 : 0;
            rightBumper = GameData.gameController.takePlayerInput ? Input.GetButton("Kick") : false;
        }
        else
        {
            rightJoystickHorizontal = GameData.gameController.takePlayerInput ? Input.GetAxis("RJoystickH") : 0;
            rightJoystickVertical = GameData.gameController.takePlayerInput ? -Input.GetAxis("RJoystickV") : 0;
            leftJoystickHorizontal = GameData.gameController.takePlayerInput ? Input.GetAxis("LJoystickH") : 0;
            leftJoystickVertical = GameData.gameController.takePlayerInput ? Input.GetAxis("LJoystickV") : 0;

            rightTriggerAxis = GameData.gameController.takePlayerInput ? Input.GetAxis("RTAxis") : 0;
            leftTriggerAxis = GameData.gameController.takePlayerInput ? Input.GetAxis("LTAxis") : 0;
            rightBumper = GameData.gameController.takePlayerInput ? Input.GetButton("RBButton") : false;
        }

        jump = GameData.gameController.takePlayerInput ? Input.GetButton("AButton") || Input.GetButton("Dash") ? true : false : false;

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

        if (ltaFlag && !leftTriggerDown && leftTriggerAxis > 0.1f)
        {
            leftTriggerDown = true;
        }
        else if (leftTriggerDown)
        {
            leftTriggerDown = false;
            ltaFlag = false;
        }

        if (leftTriggerAxis <= 0.1f)
        {
            ltaFlag = true;
        }
    }

    public bool TractionButtonPressed()
    {
        return rightTriggerAxis == 1 ? true : false;
    }
}
