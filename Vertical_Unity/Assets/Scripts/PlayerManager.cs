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
    [Header("General debug settings")]
    public Color stunColor;
    public bool vulnerable;

    [HideInInspector] public bool isVulnerable;
    [HideInInspector] public bool isDodging;
    [HideInInspector] public bool isStunned;
    [HideInInspector] public int currentHealth;  // à récupérer pour l'interface
    [HideInInspector] public int currentEnergy;
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
        invulnerableTimeRemaining = 0;
        currentEnergy = baseEnergy;
        hpIconsState = new HpState[maxhealthPoint];
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
        if(isVulnerable && !isDodging)
        {
            tookDamage = true;
            currentHealth -= damage;
            GameData.playerMovement.Propel(knockBack, false, false);
            GameData.playerGrapplingHandler.ReleaseHook();
            StartCoroutine(Stun(stunTime));
            invulnerableTimeRemaining = invulnerableTime;
            isVulnerable = false;
            if (currentHealth <= 0)
            {
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
        GetComponentInChildren<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        rb.simulated = false;
        GameData.playerMovement.inControl = false;
        GameData.playerGrapplingHandler.canShoot = false;

        yield return new WaitForSeconds(0);
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

    private void UpdateHealthBar()
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
