using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPortalHandler : MonoBehaviour
{
    public Vector2 cinematicCameraOffset;
    public float cinematicCameraSize;
    public float cinematicCameraLerpSpeed;
    public float timeBeforeDestruction;
    public float timeBeforeVictory;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public IEnumerator End()
    {
        GameData.gameController.takePlayerInput = false;
        StartCoroutine(GameData.cameraHandler.CinematicLook((Vector2)transform.position + cinematicCameraOffset, timeBeforeDestruction + timeBeforeVictory, cinematicCameraSize, cinematicCameraLerpSpeed));
        yield return new WaitForSeconds(timeBeforeDestruction);
        animator.SetBool("IsDestroyed", true);
        yield return new WaitForSeconds(timeBeforeVictory);
        /*StartCoroutine(GameData.cameraHandler.CinematicLook(GameData.playerMovement.transform.position, 100, 3, cinematicCameraLerpSpeed));
        yield return new WaitForSeconds(0.5f);
        GameData.playerVisuals.isCastingPower = 5;
        yield return new WaitForSeconds(1);*/
        GameData.gameController.LoadNextLevel();
    }
}
