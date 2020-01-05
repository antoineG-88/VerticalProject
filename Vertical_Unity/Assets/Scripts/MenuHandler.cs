using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    public GameObject PausePanel;

    private bool menuActive;

    private void Update()
    {
        if (!menuActive && Input.GetButtonDown("MenuButton"))
        {
            Pause();
        }
        else if(menuActive && Input.GetButtonDown("MenuButton"))
        {
            Resume();
        }
    }

    public void Pause()
    {
        PausePanel.SetActive(true);
        menuActive = true;
        GameData.gameController.pause = true;
        Time.timeScale = 0;
    }

    public void Resume()
    {
        PausePanel.SetActive(false);
        menuActive = false;
        GameData.gameController.pause = false;
        Time.timeScale = 1;
    }

    public void GoToMainMenu()
    {
        Resume();
        SceneManager.LoadScene(0);
    }
}
