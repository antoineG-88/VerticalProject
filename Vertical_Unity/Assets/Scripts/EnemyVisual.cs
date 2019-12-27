using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVisual : MonoBehaviour
{
    private EnemyHandler enemyHandler;
    private bool spriteFacingRight;
    void Start()
    {
        enemyHandler = GetComponent<EnemyHandler>();
        spriteFacingRight = true;
    }

    void Update()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if(enemyHandler.facingRight != spriteFacingRight)
        {
            spriteFacingRight = enemyHandler.facingRight;
            FlipTransform(enemyHandler.facingRight);
        }
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
