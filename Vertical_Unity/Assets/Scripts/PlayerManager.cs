using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [Header("Initial Player Info")]
    public int maxhealthPoint;
    public int baseEnergy;
    public float invulnerableTime;
    public bool firstLevel;
    [Header("HUD settings")]
    public Image powerCharge;
    public Image currentPowerIcon;
    public Color coolingPowerColor;
    public RectTransform firstHealthPointPos;
    public float distanceBetweenHp;
    public GameObject hpIconPrefab;
    public Sprite fullHp;
    public Sprite halfHp;
    public Color emptyHpColor;
    public Text currentEnergyText;
    public GameObject gameOverPanel;
    public Text gameOverText;
    public float gameOverFadingSpeed;
    public float timeBeforeGameOverTextFade;
    public Text restartText;
    public float timeBeforeRestartPossible;
    [Header("General debug settings")]
    public Color stunColor;
    public bool vulnerable;

    [HideInInspector] public bool isVulnerable;
    [HideInInspector] public bool isDodging;
    [HideInInspector] public bool isStunned;
    [HideInInspector] public int currentHealth;  // à récupérer pour l'interface
    [HideInInspector] public int currentEnergy;
    [HideInInspector] public bool isDead;
    private Rigidbody2D rb;
    private Color baseColor;
    private float invulnerableTimeRemaining;
    private List<GameObject> hpIcons = new List<GameObject>();
    private HpState[] hpIconsState;

    private enum HpState { Full, Half, Empty };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if(firstLevel)
        {
            currentHealth = maxhealthPoint * 2;
        }
        else
        {
            currentHealth = PlayerData.playerHealth;
        }
        baseColor = GetComponentInChildren<SpriteRenderer>().color;
        isDodging = false;
        isStunned = false;
        isDead = false;
        invulnerableTimeRemaining = 0;
        currentEnergy = baseEnergy;
        hpIconsState = new HpState[maxhealthPoint];
        gameOverPanel.SetActive(false);
        InitializeHealthBar();
    }

    private void Update()
    {
        if (!GameData.gameController.pause)
        {
            UpdatePlayerStatus();
            UpdateHUD();

            if (Input.GetKeyDown(KeyCode.M))
            {
                PlayerData.playerHealth = currentHealth;
                Debug.Log("CurrentHealth : " + currentHealth + " saved in PlayerData");
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("PlayerData health : " + PlayerData.playerHealth);
            }
        }
    }

    public bool TakeDamage(int damage, Vector2 knockBack, float stunTime)
    {
        bool tookDamage = false;
        if(isVulnerable && !isDodging && !isDead)
        {
            tookDamage = true;
            currentHealth -= damage;
            GameData.playerMovement.Propel(knockBack, false, false);
            GameData.playerGrapplingHandler.ReleaseHook();
            GameData.playerVisuals.isHurt = 10;
            StartCoroutine(Stun(stunTime));
            StartCoroutine(GameData.gameController.postProcessHandler.TriggerHurtEffect());
            invulnerableTimeRemaining = invulnerableTime;
            isVulnerable = false;
            if (currentHealth <= 0)
            {
                isDead = true;
                StartCoroutine(Die());
            }
        }
        UpdateHealthBar();
        return tookDamage;
    }

    public IEnumerator Stun(float stunTime)
    {
        GetComponentInChildren<SpriteRenderer>().color = stunColor;
        GameData.playerMovement.inControl = false;
        GameData.playerGrapplingHandler.canShoot = false;
        isStunned = true;
        yield return new WaitForSeconds(stunTime);
        GetComponentInChildren<SpriteRenderer>().color = baseColor;
        isStunned = false;
        GameData.playerMovement.inControl = true;
        GameData.playerGrapplingHandler.canShoot = true;
    }

    private void UpdatePlayerStatus()
    {
        if (invulnerableTimeRemaining > 0)
        {
            invulnerableTimeRemaining -= Time.deltaTime;
        }
        else
        {
            isVulnerable = true;
        }

        isVulnerable = vulnerable ? isVulnerable : false;
    }

    private IEnumerator Die()
    {
        GameData.playerMovement.inControl = false;
        GameData.playerGrapplingHandler.canShoot = false;
        GameData.gameController.takePlayerInput = false;
        float timer = 0;
        while ((!GameData.playerMovement.IsOnGround() && GameData.playerMovement.transform.position.y > GameData.levelBuilder.bottomCenterTowerPos.y) || timer < 0.2f)
        {
            GameData.playerVisuals.isHurt = 3;
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
        }
        StartCoroutine(GameData.cameraHandler.CinematicLook(transform.position, 1000.0f, 2.0f, 4.0f));
        StartCoroutine(GameData.gameController.postProcessHandler.ActivateDeathVignette());
        GameData.playerVisuals.isHurt = 0;

        yield return new WaitForSeconds(timeBeforeGameOverTextFade);

        gameOverPanel.transform.parent.gameObject.SetActive(true);
        gameOverPanel.SetActive(true);

        gameOverText.color = new Color(gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, 0);
        restartText.color = new Color(restartText.color.r, restartText.color.g, restartText.color.b, 0);
        while (gameOverText.color.a <= 0.95f)
        {
            gameOverText.color = new Color(gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, gameOverText.color.a + Time.deltaTime * gameOverFadingSpeed);
            yield return new WaitForEndOfFrame();
        }

        gameOverText.color = new Color(gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, 1);
        yield return new WaitForSeconds(timeBeforeRestartPossible);
        while (restartText.color.a <= 0.95f)
        {
            restartText.color = new Color(restartText.color.r, restartText.color.g, restartText.color.b, restartText.color.a + Time.deltaTime * gameOverFadingSpeed);
            yield return new WaitForEndOfFrame();
        }

        restartText.color = new Color(restartText.color.r, restartText.color.g, restartText.color.b, 1);

        while (!Input.anyKey)
        {
            yield return new WaitForEndOfFrame();
        }

        GameData.gameController.RestartLevel();
    }

    public void EarnEnergy(int energyEarned)
    {
        currentEnergy += energyEarned;
    }

    public void PayEnergy(int cost)
    {
        currentEnergy -= cost;
    }

    private void UpdateHUD()
    {
        currentEnergyText.text = currentEnergy.ToString();
        powerCharge.fillAmount = (GameData.playerAttackManager.currentPower.cooldown - GameData.playerAttackManager.powerCooldownRemaining) / GameData.playerAttackManager.currentPower.cooldown;
        if(powerCharge.fillAmount < 1)
        {
            currentPowerIcon.color = coolingPowerColor;
        }
        else
        {
            currentPowerIcon.color = Color.white;
        }
        currentPowerIcon.sprite = GameData.playerAttackManager.currentPower.icon;
    }

    private void InitializeHealthBar()
    {
        for (int i = 0; i < maxhealthPoint; i++)
        {
            GameObject newIcon;
            hpIcons.Add(newIcon = Instantiate(hpIconPrefab, firstHealthPointPos));
            newIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(distanceBetweenHp * i, 0.0f);
        }
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        int hpRemaining = currentHealth;
        for (int i = 0; i < maxhealthPoint; i++)
        {
            if(hpRemaining >= 2)
            {
                hpIconsState[i] = HpState.Full;
                hpRemaining -= 2;
            }
            else if (hpRemaining == 1)
            {
                hpIconsState[i] = HpState.Half;
                hpRemaining--;
            }
            else if (hpRemaining <= 0)
            {
                hpIconsState[i] = HpState.Empty;
            }
        }

        for (int i = 0; i < hpIconsState.Length; i++)
        {
            Image icon = hpIcons[i].GetComponent<Image>();
            switch (hpIconsState[i])
            {
                case HpState.Full:
                    icon.sprite = fullHp;
                    icon.color = Color.white;
                    break;

                case HpState.Half:
                    icon.sprite = halfHp;
                    icon.color = Color.white;
                    break;

                case HpState.Empty:
                    icon.sprite = fullHp;
                    icon.color = emptyHpColor;
                    break;
            }
        }
    }
}
