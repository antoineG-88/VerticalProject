using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PicTrap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {

        GameData.playerManager.TakeDamage(8, new Vector2(2f,13f),0);
    }
}

