using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingHandler : MonoBehaviour
{
    public PlayerGrapplingHandler playerGrapplingHandler;

    void Start()
    {
        playerGrapplingHandler = GameObject.FindWithTag("Player").GetComponent<PlayerGrapplingHandler>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.CompareTag("Hook"))
        {
            playerGrapplingHandler.AttachHook(gameObject);
        }
    }
}
