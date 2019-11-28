using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    public Power currentPower;
    [Space]
    public Kick currentKick;
    [Space]
    public Vector2 collideSize;
    [Space]
    public GameObject kickPreview;
    [Space]
    public bool reAimMode;
    public GameObject repropulsionPreview;
    public float maxRepropulsionReleaseTime;
    public float maxReAimingTime;
    [Range(0.0001f,1.0f)] public float slowMoTimeSpeed;

    [HideInInspector] public bool isKicking;
    [HideInInspector] public bool kickButtonPressed;
    private float remainingTimeBeforeKick;
    private bool isRepropulsing;
    private bool isReAiming;
    private float powerCooldownRemaining;

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
        Input();
    }

    private void Input()
    {
        if (GameData.gameController.input.rightBumper)
        {
            kickButtonPressed = true;
            if(GameData.playerGrapplingHandler.isTracting && !isKicking)
            {
                isKicking = true;
                StartCoroutine(currentKick.Use(gameObject, Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.right, GameData.playerGrapplingHandler.tractionDirection))));
            }
        }
        else
        {
            kickButtonPressed = false;
        }

        if(GameData.gameController.input.leftTriggerAxis > 0 && powerCooldownRemaining <= 0)
        {
            powerCooldownRemaining = currentPower.cooldown;
            StartCoroutine(currentPower.Use());
        }

        if(powerCooldownRemaining > 0)
        {
            powerCooldownRemaining -= Time.deltaTime;
        }
    }

    public bool Hit()
    {
        bool successfullKick = false;
        List<Collider2D> hitColliders = new List<Collider2D>();
        GameObject attachedObject = GameData.playerGrapplingHandler.attachedObject;
        GameData.playerGrapplingHandler.ReleaseHook();

        if (currentKick.HitTest(attachedObject, ref hitColliders) && attachedObject.CompareTag("Enemy"))
        {
            EnemyHandler enemy = attachedObject.GetComponent<EnemyHandler>();
            successfullKick = !enemy.TestCounter();
            if (successfullKick)
            {
                if (!currentKick.isAOE)
                {
                    currentKick.DealDamageToEnemy(enemy);
                }
                else
                {
                    foreach(Collider2D collider in hitColliders)
                    {
                        if(collider.CompareTag("Enemy"))
                        {
                            currentKick.DealDamageToEnemy(collider.GetComponent<EnemyHandler>());
                        }
                    }
                }

                if(Vector2.Distance(transform.position, attachedObject.transform.position) < currentKick.perfectTimingMaximumDistance)
                {
                    currentKick.ApplyPerfectTimingEffect(enemy);
                }

                if(reAimMode)
                {
                    StartCoroutine(ReAim());
                }
                else
                {
                    StartCoroutine(Repropulsion());
                }
            }
        }
        isKicking = false;

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
            if(Mathf.Abs(GameData.gameController.input.rightJoystickHorizontal) > 0.1f || Mathf.Abs(GameData.gameController.input.rightJoystickVertical) > 0.1f)
            {
                aimDirection = new Vector2(GameData.gameController.input.rightJoystickHorizontal, GameData.gameController.input.rightJoystickVertical).normalized;
                repropulsionPreview.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(aimDirection.x, - aimDirection.y) * 180 / Mathf.PI - 90);
                repropulsionPreview.SetActive(true);
            }
            else
            {
                repropulsionPreview.SetActive(false);
            }

            if (!GameData.gameController.input.rightBumper)
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

    private IEnumerator ReAim()
    {
        isReAiming = true;
        GameData.playerGrapplingHandler.timeBeforeNextShoot = 0;
        Time.timeScale = slowMoTimeSpeed;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        float timeRemaining = Time.realtimeSinceStartup + maxReAimingTime;

        while (isReAiming && timeRemaining > Time.realtimeSinceStartup)
        {
            yield return new WaitForEndOfFrame();

            if (GameData.playerGrapplingHandler.currentHook != null)
            {
                isReAiming = false;
            }
        }

        isReAiming = false;
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
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

    public Power ReplaceCurrentPower(Power newPower)
    {
        Power previousPower = currentPower;
        currentPower = newPower;
        return previousPower;
    }
}
