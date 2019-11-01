using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    public Kick currentKick;
    [Header("Collide settings")]
    public Vector2 collideSize;
    public GameObject kickPreview;

    [HideInInspector] public bool isKicking;
    [HideInInspector] public bool kickUsed;
    private float remainingTimeBeforeKick;

    private void Start()
    {
        isKicking = false;
        kickUsed = false;
        kickPreview.SetActive(false);
        kickPreview.transform.localScale = currentKick.hitCollidingSize;
    }

    private void Update()
    {
        KickTest();
    }

    private void FixedUpdate()
    {
        /*Collider2D enemyCollider = Physics2D.OverlapBox(transform.position, collideSize, 0.0f, LayerMask.GetMask("Ennemy"));
        if (enemyCollider != null && GameData.playerGrapplingHandler.isTracting && GameData.playerGrapplingHandler.attachedObject == enemyCollider.gameObject)
        {
            TriggerKick(enemyCollider.GetComponent<EnnemyHandler>());
        }*/
    }

    private void KickTest()
    {
        if (Input.GetAxisRaw("RTAxis") == 1 && GameData.playerGrapplingHandler.isTracting && !isKicking && !kickUsed)
        {
            kickUsed = true;
            remainingTimeBeforeKick = currentKick.timeBeforeKick;
            isKicking = true;
            kickPreview.SetActive(true);
        }

        if (remainingTimeBeforeKick > 0)
        {
            remainingTimeBeforeKick -= Time.deltaTime;
        }

        if(remainingTimeBeforeKick <= 0 && isKicking)
        {
            Collider2D testedCollider = currentKick.HitTest();
            if (testedCollider != null)
            {
                TriggerKick(testedCollider.GetComponent<EnnemyHandler>());
            }
            isKicking = false;
            GameData.playerGrapplingHandler.ReleaseHook();
            kickPreview.SetActive(false);
        }
        
        if(isKicking)
        {
            kickPreview.transform.position = (Vector2)GameData.playerMovement.transform.position + GameData.playerGrapplingHandler.tractionDirection * currentKick.hitCollidingSize.x / 2;
            kickPreview.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Vector2.Angle(Vector2.right, GameData.playerGrapplingHandler.tractionDirection));
        }
    }

    /// <summary>
    /// Replace the equipped Kick with a new one, return the kick replaced
    /// </summary>
    /// <param name="newKick">The kick that will be replacing the old one</param>
    /// <returns></returns>
    public Kick ReplaceCurrentKick(Kick newKick)
    {
        Kick previousKick = currentKick;
        currentKick = newKick;
        return previousKick;
    }

    public void TriggerKick(EnnemyHandler ennemy)
    {
        currentKick.Use(ennemy);
    }
}
