using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Header("Timing settings")]
    public float timeBeforePowerSpawn;
    public float timeBeforePrisonOpening;
    public float timeBetweenPowerSpawn;
    [Header("Selling settings")]
    public float minDistanceSelectPower;
    public float selectSize;
    public Vector2 powerIconOffset;

    private bool prisonOpened;
    private GameObject firstPowerO;
    private Power firstPower;
    private GameObject secondPowerO;
    private Power secondPower;
    private List<Power> availablePowers;

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
                if(Input.GetButtonDown("Interact"))
                {
                    Instantiate(GameData.gameController.debubParticle, firstPowerSpawnPoint);
                    BuyPower(firstPower);
                }
            }
            else
            {
                firstPowerWindow.SetBool("Selected", false);
            }

            if (Vector2.Distance(secondPowerSpawnPoint.position, GameData.playerMovement.transform.position) < minDistanceSelectPower)
            {
                secondPowerWindow.SetBool("Selected", true);
                if (Input.GetButtonDown("Interact"))
                {
                    Instantiate(GameData.gameController.debubParticle, secondPowerSpawnPoint);
                    BuyPower(secondPower);
                }
            }
            else
            {
                secondPowerWindow.SetBool("Selected", false);
            }
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
        firstPower = availablePowers[Random.Range(0, availablePowers.Count)];
        availablePowers.Remove(firstPower);
        secondPower = availablePowers[Random.Range(0, availablePowers.Count)];
        firstPowerO = Instantiate(pickablePowerPrefab, (Vector2)firstPowerSpawnPoint.position + powerIconOffset, Quaternion.identity, firstPowerSpawnPoint);
        firstPowerO.GetComponent<SpriteRenderer>().sprite = firstPower.icon;
        firstPowerWindow.SetBool("Closed", false);
        yield return new WaitForSeconds(timeBetweenPowerSpawn);
        secondPowerO = Instantiate(pickablePowerPrefab, (Vector2)secondPowerSpawnPoint.position + powerIconOffset, Quaternion.identity, secondPowerSpawnPoint);
        secondPowerO.GetComponent<SpriteRenderer>().sprite = secondPower.icon;
        secondPowerWindow.SetBool("Closed", false);
        isSelling = true;

        GameData.gameController.takePlayerInput = true;
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
        GameData.playerAttackManager.ReplaceCurrentPower(newPower);
        isSelling = false;
        firstPowerWindow.SetBool("Closed", true);
        secondPowerWindow.SetBool("Closed", true);
        Destroy(firstPowerO);
        Destroy(secondPowerO);
        capturedYokai.SetBool("Disappear", true);
    }
}
