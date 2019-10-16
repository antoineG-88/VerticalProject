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
    public float hitCollidingCenterDistance;

    public abstract void Use(EnnemyHandler ennemy);

    public Collider2D HitTest()
    {
        Collider2D collider = Physics2D.OverlapBox((Vector2)GameData.playerMovement.transform.position + GameData.playerGrapplingHandler.tractionDirection * hitCollidingSize.x / 2, hitCollidingSize, Vector2.Angle(Vector2.right, GameData.playerGrapplingHandler.tractionDirection));
        if(collider.CompareTag("Ennemy"))
        {
            return collider;
        }
        return null;
    }


}
