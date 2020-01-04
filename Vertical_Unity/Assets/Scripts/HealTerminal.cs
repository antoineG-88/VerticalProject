using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealTerminal : MonoBehaviour
{
    public int lifePrice;
    GameObject interactButton;
    public int lifeAdded;
    bool isUsed;

    private void Start()
    {
        isUsed = true;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (isUsed == true)
        {

            if (Input.GetButton("Interact") == true)
            {
                GameData.playerManager.currentEnergy = GameData.playerManager.currentEnergy - lifePrice;
                GameData.playerManager.currentHealth = GameData.playerManager.currentHealth + lifeAdded;
                GameData.playerManager.UpdateHealthBar();
                isUsed = false;
            }
        }
    }
}
