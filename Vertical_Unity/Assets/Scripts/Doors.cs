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
    public AudioClip doorOpenClip;
    public AudioClip doorCloseClip;

    private bool isOpened;
    private AudioSource source;

    private void Start()
    {
        isOpened = false;
        source = GetComponent<AudioSource>();
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
        if (collider.CompareTag("Player") && !isLocked && !isOpened)
        {
            source.PlayOneShot(doorOpenClip);
            isOpened = true;
            doorCollisionCollider.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            source.PlayOneShot(doorCloseClip);
            isOpened = false;
            doorCollisionCollider.enabled = true;
        }
    }
}
