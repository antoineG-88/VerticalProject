using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Kick : ScriptableObject
{
    public new string name;
    [TextArea] public string description;
    public Sprite icon;
    public GameObject kickVisualEffect;

    public float propelingForce;
    public Vector2 hitCollidingSize;
    public float timeBeforeKick;

    private Collider2D hitCollider;
    
    public abstract IEnumerator Use(GameObject player, Quaternion kickRotation);

    public abstract void DealDamageToEnemy(EnemyHandler enemy);

    public GameObject HitTest(LayerMask layerTested)
    {
        GameObject objectFound = null;
        hitCollider = Physics2D.OverlapBox((Vector2)GameData.playerMovement.transform.position + GameData.playerGrapplingHandler.tractionDirection * hitCollidingSize.x / 2, hitCollidingSize, Vector2.SignedAngle(Vector2.right, GameData.playerGrapplingHandler.tractionDirection), layerTested);
        if (hitCollider != null)
        {
            objectFound = hitCollider.gameObject;
        }

        return objectFound;
    }
}
