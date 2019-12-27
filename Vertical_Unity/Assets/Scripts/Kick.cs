using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Kick : ScriptableObject
{
    public new string name;
    [TextArea] public string description;
    public Sprite icon;
    public GameObject kickVisualEffect;

    public bool isAOE;
    public float propelingForce;
    public float perfectTimingMaximumDistance;
    public Vector2 hitCollidingSize;
    public float timeBeforeKick;

    public abstract IEnumerator Use(GameObject player, Quaternion kickRotation);

    public abstract void DealDamageToEnemy(EnemyHandler enemy);

    public abstract void ApplyPerfectTimingEffect(EnemyHandler enemy);

    public bool HitTest(GameObject attachedObject, ref List<Collider2D> overlappingColliders)
    {
        bool hasHit = false;
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.NoFilter();
        List<Collider2D> colliders = new List<Collider2D>();
        Physics2D.OverlapBox((Vector2)GameData.playerMovement.transform.position + GameData.playerGrapplingHandler.tractionDirection * hitCollidingSize.x / 2, hitCollidingSize, Vector2.SignedAngle(Vector2.right, GameData.playerGrapplingHandler.tractionDirection), contactFilter, colliders);
        foreach(Collider2D collider in colliders)
        {
            if(collider.gameObject == attachedObject)
            {
                hasHit = true;
            }
        }
        overlappingColliders = colliders;

        return hasHit;
    }
}
