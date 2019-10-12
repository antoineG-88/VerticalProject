using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        GameData.Initialize(player.GetComponent<PlayerManager>(), player.GetComponent<PlayerMovement>(), player.GetComponent<PlayerGrapplingHandler>(), player.GetComponent<PlayerAttackManager>());
    }
}
