using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingHandler : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.CompareTag("Hook"))
        {
            GameData.playerGrapplingHandler.AttachHook(gameObject);
        }
    }
}
