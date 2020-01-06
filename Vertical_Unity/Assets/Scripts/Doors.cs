using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doors : MonoBehaviour
{
    public Animator doorUp;
    public Animator doorDown;
    public BoxCollider2D triggerBox;
    public Collider2D doorCollisionCollider;
    public bool isLocked;

    private bool isOpened;

    private void Start()
    {
        isOpened = false;
    }

    private void FixedUpdate()
    {
        doorUp.SetBool("IsLocked", isLocked);
        doorDown.SetBool("IsLocked", isLocked);
        doorUp.SetBool("opened", isOpened);
        doorDown.SetBool("opened", isOpened);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && !isLocked)
        {
            isOpened = true;
            doorCollisionCollider.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            isOpened = false;
            doorCollisionCollider.enabled = true;
        }
    }
}
