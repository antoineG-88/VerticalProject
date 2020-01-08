using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealTerminal : MonoBehaviour
{
    public int lifePrice;
    public int lifeAdded;
    bool isUsed;

    private void Start()
    {
        isUsed = false;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (isUsed == false)
        {

            if (Input.GetButton("Interact") == true && GameData.playerManager.currentEnergy >= lifePrice)
            {
                GameData.playerManager.currentEnergy -= lifePrice;
                GameData.playerManager.currentHealth += lifeAdded;
                if(GameData.playerManager.currentHealth > GameData.playerManager.maxhealthPoint * 2)
                {
                    GameData.playerManager.currentHealth = GameData.playerManager.maxhealthPoint * 2;
                }
                GameData.playerManager.UpdateHealthBar();
                isUsed = true;

                GetComponentInChildren<Animator>().SetBool("Used", true);
            }
        }
    }
}
