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
    [Space]
    public GameObject startButtonImage;
    public float startButtonLoopSpeed;
    public float startButtonLoopSize;

    private Vector3 startButtonOriginScale;

    private void Start()
    {
        startButtonOriginScale = startButtonImage.transform.localScale;
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
