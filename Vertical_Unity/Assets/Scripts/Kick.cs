using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Kick : ScriptableObject
{
    public new string name;
    public int level;
    public Sprite icon;

    public float propelingForce;
    public Vector2 hitCollidingSize;
    public float timeBeforeKick;

    private Collider2D hitCollider;
    
    public abstract void Use(EnnemyHandler ennemy);

    public Collider2D HitTest()
    {
        hitCollider = Physics2D.OverlapBox((Vector2)GameData.playerMovement.transform.position + GameData.playerGrapplingHandler.tractionDirection * hitCollidingSize.x / 2, hitCollidingSize, Vector2.SignedAngle(Vector2.right, GameData.playerGrapplingHandler.tractionDirection), LayerMask.GetMask("Ennemy","Ring"));
        if (hitCollider != null && hitCollider.CompareTag("Ennemy"))
        {
            return hitCollider;
        }

        return null;
    }
}
