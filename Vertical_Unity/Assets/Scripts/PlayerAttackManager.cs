﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    public Kick currentKick;
    [Space]
    public Vector2 collideSize;
    [Space]
    public GameObject kickPreview;
    [Space]
    public GameObject repropulsionPreview;
    public float maxRepropulsionReleaseTime;
    [Range(0.0001f,1.0f)] public float slowMoTimeSpeed;

    [HideInInspector] public bool isKicking;
    private float remainingTimeBeforeKick;
    private bool isRepropulsing;

    private void Start()
    {
        isKicking = false;
        kickPreview.SetActive(false);
        kickPreview.transform.localScale = currentKick.hitCollidingSize;
        repropulsionPreview.SetActive(false);
        isRepropulsing = false;
    }

    private void Update()
    {
        KickInput();
    }

    private void KickInput()
    {
        if (Input.GetButton("RBButton") && GameData.playerGrapplingHandler.isTracting && !isKicking)
        {
            isKicking = true;
            StartCoroutine(currentKick.Use(gameObject, Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.right, GameData.playerGrapplingHandler.tractionDirection))));
        }
        
        /*if(isKicking)
        {
            kickPreview.transform.position = (Vector2)GameData.playerMovement.transform.position + GameData.playerGrapplingHandler.tractionDirection * currentKick.hitCollidingSize.x / 2;
            kickPreview.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.right, GameData.playerGrapplingHandler.tractionDirection));
        }*/
    }

    public bool Hit()
    {
        bool successfullKick = false;
        GameObject objectHit = currentKick.HitTest(LayerMask.GetMask("Enemy"));

        if (objectHit != null)
        {
            EnemyHandler enemy = objectHit.GetComponent<EnemyHandler>();
            successfullKick = !enemy.TestCounter();
            if(successfullKick)
            {
                currentKick.DealDamageToEnemy(enemy);
                StartCoroutine(Repropulsion());
            }
        }
        isKicking = false;
        GameData.playerGrapplingHandler.ReleaseHook();

        return successfullKick;
    }

    private IEnumerator Repropulsion()
    {
        isRepropulsing = true;
        Time.timeScale = slowMoTimeSpeed;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        float timeRemaining = Time.realtimeSinceStartup + maxRepropulsionReleaseTime;
        Vector2 aimDirection = Vector2.zero;
        GameData.playerGrapplingHandler.canShoot = false;

        while (isRepropulsing && timeRemaining > Time.realtimeSinceStartup)
        {
            yield return new WaitForEndOfFrame();

            aimDirection = Vector2.zero;
            if(Mathf.Abs(GameData.gameController.rightJoystickHorizontal) > 0.1f || Mathf.Abs(GameData.gameController.rightJoystickVertical) > 0.1f)
            {
                aimDirection = new Vector2(GameData.gameController.rightJoystickHorizontal, GameData.gameController.rightJoystickVertical).normalized;
                repropulsionPreview.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(aimDirection.x, - aimDirection.y) * 180 / Mathf.PI - 90);
                repropulsionPreview.SetActive(true);
            }
            else
            {
                repropulsionPreview.SetActive(false);
            }

            if (!GameData.gameController.rightBumper)
            {
                isRepropulsing = false;
            }
        }

        isRepropulsing = false;
        repropulsionPreview.SetActive(false);
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        GameData.playerGrapplingHandler.canShoot = true;

        if (aimDirection != Vector2.zero)
        {
            GameData.playerMovement.Propel(aimDirection * currentKick.propelingForce, true, true);
        }
        else
        {
            GameData.playerMovement.Propel(Vector2.up * currentKick.propelingForce, true, true);
        }
    }

    /// <summary>
    /// Replace the equipped Kick with a new one, return the kick replaced
    /// </summary>
    /// <param name="newKick">The kick that will be replacing the old one</param>
    /// <returns></returns>
    public Kick ReplaceCurrentKick(Kick newKick)
    {
        Kick previousKick = currentKick;
        currentKick = newKick;
        return previousKick;
    }
}
