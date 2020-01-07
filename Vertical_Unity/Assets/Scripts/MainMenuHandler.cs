using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    public int indexStartTowerScene;
    [Space]
    public GameObject startPanel;
    public GameObject mainMenuPanel;
    public GameObject creditsPanel;
    public GameObject yokai;
    public float yokaiParallaxStrength;
    public float yokaiLerpSpeed;
    [Space]
    public GameObject startButtonImage;
    public float startButtonLoopSpeed;
    public float startButtonLoopSize;

    private Vector3 startButtonOriginScale;
    private Vector2 yokaiStartPos;
    private Vector2 yokaiTargetPos;

    private void Start()
    {
        startButtonOriginScale = startButtonImage.transform.localScale;
        yokaiStartPos = yokai.transform.position;
    }

    private void Update()
    {
        if((Input.anyKeyDown && startPanel.activeSelf) || (mainMenuPanel.activeSelf && Input.GetButtonDown("MenuButton")))
        {
            SwitchMainMenu();
        }

        if(Input.GetButtonDown("MenuButton") && creditsPanel.activeSelf)
        {
            SetCredits(false);
        }

        startButtonImage.transform.localScale = startButtonOriginScale + startButtonOriginScale * Mathf.Cos(Time.realtimeSinceStartup * startButtonLoopSpeed) * startButtonLoopSize;

        yokaiTargetPos = yokaiStartPos + ((Vector2)Input.mousePosition - new Vector2(Camera.main.scaledPixelWidth / 2, Camera.main.scaledPixelHeight)) * yokaiParallaxStrength;

        yokai.transform.position = Vector2.Lerp(yokai.transform.position, yokaiTargetPos, yokaiLerpSpeed);
    }

    public void SwitchMainMenu()
    {
        if(mainMenuPanel.activeSelf)
        {
            mainMenuPanel.SetActive(false);
            startPanel.SetActive(true);
        }
        else
        {
            mainMenuPanel.SetActive(true);
            startPanel.SetActive(false);
        }
    }

    public void StartRun()
    {
        SceneManager.LoadScene(indexStartTowerScene);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void SetCredits(bool active)
    {
        creditsPanel.SetActive(active);
    }
}
