using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public bool useMouseAim;
    public bool useMouseMovement;

    [HideInInspector] public float rightJoystickHorizontal;
    [HideInInspector] public float rightJoystickVertical;
    [HideInInspector] public float rightTriggerAxis;
    [HideInInspector] public bool rightTriggerDown;
    [HideInInspector] public bool rightBumper;

    private bool rtaFlag;

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

            rightTriggerAxis = Input.GetMouseButton(0) ? 1 : 0;
            rightBumper = Input.GetMouseButton(1);
        }
        else
        {
            rightJoystickHorizontal = Input.GetAxis("RJoystickH");
            rightJoystickVertical = -Input.GetAxis("RJoystickV");

            rightTriggerAxis = Input.GetAxis("RTAxis");
            rightBumper = Input.GetButton("RBButton");
        }

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
