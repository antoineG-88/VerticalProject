using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YokaiPrisonHandler : MonoBehaviour
{
    [Header("References")]
    public Animator yokaiPrison;
    public Animator capturedYokai;
    public Transform firstPowerSpawnPoint;
    public Animator firstPowerWindow;
    public Transform secondPowerSpawnPoint;
    public Animator secondPowerWindow;
    public GameObject pickablePowerPrefab;
    public GameObject energyPriceText;
    public GameObject yokaiCanvas;
    [Header("Timing settings")]
    public float timeBeforePowerSpawn;
    public float timeBeforePrisonOpening;
    public float timeBetweenPowerSpawn;
    [Header("Selling settings")]
    public float minDistanceSelectPower;
    public float selectSize;
    public Vector2 powerIconOffset;
    public Vector2 powerPriceOffset;

    private bool prisonOpened;
    private GameObject firstPowerO;
    private Power firstPower;
    private GameObject secondPowerO;
    private Power secondPower;
    private List<Power> availablePowers;
    private GameObject firstPrice;
    private GameObject secondPrice;

    private bool isSelling;

    void Start()
    {
        yokaiPrison.SetBool("Opened", false);
        prisonOpened = false;
        isSelling = false;
        firstPowerWindow.SetBool("Closed", true);
        secondPowerWindow.SetBool("Closed", true);
    }

    private void Update()
    {
        if(isSelling)
        {
            if(Vector2.Distance(firstPowerSpawnPoint.position, GameData.playerMovement.transform.position) < minDistanceSelectPower)
            {
                firstPowerWindow.SetBool("Selected", true);
                firstPrice.SetActive(true);
                if(Input.GetButtonDown("Interact"))
                {
                    BuyPower(firstPower);
                }
            }
            else
            {
                firstPowerWindow.SetBool("Selected", false);
                firstPrice.SetActive(false);
            }

            if (Vector2.Distance(secondPowerSpawnPoint.position, GameData.playerMovement.transform.position) < minDistanceSelectPower)
            {
                secondPowerWindow.SetBool("Selected", true);
                secondPrice.SetActive(true);
                if (Input.GetButtonDown("Interact"))
                {
                    BuyPower(secondPower);
                }
            }
            else
            {
                secondPowerWindow.SetBool("Selected", false);
                secondPrice.SetActive(false);
            }

            if(firstPrice != null)
                SetPricePos((Vector2)firstPowerSpawnPoint.position + powerPriceOffset, firstPrice);
            if (secondPrice != null)
                SetPricePos((Vector2)secondPowerSpawnPoint.position + powerPriceOffset,secondPrice);
        }
    }

    private IEnumerator OpenPrison()
    {
        ResetPrisonValues();
        availablePowers.Remove(GameData.playerAttackManager.currentPower);
        prisonOpened = true;
        GameData.gameController.takePlayerInput = false;
        StartCoroutine(GameData.cameraHandler.CinematicLook(capturedYokai.transform.position, timeBeforePowerSpawn+ timeBeforePrisonOpening, 4, 0.1f));
        yield return new WaitForSeconds(timeBeforePrisonOpening);
        yokaiPrison.SetBool("Opened", true);
        yield return new WaitForSeconds(timeBeforePowerSpawn);
        firstPower = GetRandomAvailablePower();
        availablePowers.Remove(firstPower);
        secondPower = GetRandomAvailablePower();
        if(firstPower != null)
        {
            firstPowerO = Instantiate(pickablePowerPrefab, (Vector2)firstPowerSpawnPoint.position + powerIconOffset, Quaternion.identity, firstPowerSpawnPoint);
            firstPowerO.GetComponent<SpriteRenderer>().sprite = firstPower.icon;
            firstPowerWindow.SetBool("Closed", false);
            firstPrice = Instantiate(energyPriceText, firstPowerO.transform.position, Quaternion.identity, yokaiCanvas.transform);
            firstPrice.GetComponent<Text>().text = firstPower.powerName + System.Environment.NewLine + firstPower.price.ToString();

        }
        yield return new WaitForSeconds(timeBetweenPowerSpawn);
        if(secondPower != null)
        {
            secondPowerO = Instantiate(pickablePowerPrefab, (Vector2)secondPowerSpawnPoint.position + powerIconOffset, Quaternion.identity, secondPowerSpawnPoint);
            secondPowerO.GetComponent<SpriteRenderer>().sprite = secondPower.icon;
            secondPowerWindow.SetBool("Closed", false);
            secondPrice = Instantiate(energyPriceText, firstPowerO.transform.position, Quaternion.identity, yokaiCanvas.transform);
            secondPrice.GetComponent<Text>().text = secondPower.powerName + System.Environment.NewLine + secondPower.price.ToString();
        }
        isSelling = true;

        GameData.gameController.takePlayerInput = true;
        if(secondPower != null && firstPower == null)
        {
            BuyPower(GameData.playerAttackManager.currentPower);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && !prisonOpened)
        {
            StartCoroutine(OpenPrison());
        }
    }

    private void ResetPrisonValues()
    {
        availablePowers = GameData.gameController.allPowers;
        firstPower = null;
        secondPower = null;
        firstPowerO = null;
        secondPowerO = null;
    }

    private void BuyPower(Power newPower)
    {
        if(GameData.playerManager.currentEnergy >= newPower.price)
        {
            GameData.playerAttackManager.ReplaceCurrentPower(newPower);
            GameData.playerManager.PayEnergy(newPower.price);
            isSelling = false;
            firstPowerWindow.SetBool("Closed", true);
            secondPowerWindow.SetBool("Closed", true);
            Destroy(firstPowerO);
            Destroy(secondPowerO);
            Destroy(firstPrice);
            Destroy(secondPrice);
            capturedYokai.SetBool("Disappear", true);
        }
        else
        {
            Debug.Log("Not enough Energy");
        }
    }

    private Power GetRandomAvailablePower()
    {
        Power availablePower = null;
        if(availablePowers.Count > 0)
        {
            availablePower = availablePowers[Random.Range(0, availablePowers.Count)];
        }
        return availablePower;
    }

    private void SetPricePos(Vector2 priceWorldPos, GameObject priceText)
    {
        Vector2 screenPos = Camera.main.WorldToViewportPoint(priceWorldPos);
        Vector2 worldScreenPos = new Vector2((screenPos.x - 0.5f) * yokaiCanvas.GetComponent<RectTransform>().sizeDelta.x, (screenPos.y - 0.5f) * yokaiCanvas.GetComponent<RectTransform>().sizeDelta.y);
        priceText.GetComponent<RectTransform>().anchoredPosition = worldScreenPos;
    }
}
