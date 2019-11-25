using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [HideInInspector] public bool facingRight;

    private Animator animator;
    private bool transformFacingRight;
    void Start()
    {
        animator = GetComponent<Animator>();
        transformFacingRight = true;
    }

    void Update()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        facingRight = GameData.playerGrapplingHandler.isTracting ? (GameData.playerGrapplingHandler.tractionDirection.x > 0 ? true : false) : (GameData.playerMovement.targetVelocity.x != 0 ? (GameData.playerMovement.targetVelocity.x > 0 ? true : false) : facingRight);


        if (facingRight != transformFacingRight)
        {
            transformFacingRight = facingRight;
            FlipTransform(facingRight);
        }

        animator.SetBool("Running", Mathf.Abs(GameData.playerMovement.targetVelocity.x) != 0 ? true : false);

        animator.SetBool("IsTracting", GameData.playerGrapplingHandler.isTracting);
    }

    private void FlipTransform(bool facing)
    {
        if (facing)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
        else
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
        }
    }
}
