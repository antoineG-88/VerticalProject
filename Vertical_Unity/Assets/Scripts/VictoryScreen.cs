using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
    
public class VictoryScreen : MonoBehaviour
{
    public Text scoreText;

    private void Start()
    {
        scoreText.text = Mathf.FloorToInt(PlayerData.timeScore / 60).ToString() + " : " + PlayerData.timeScore % 60;
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(0);
        }
    }
}
