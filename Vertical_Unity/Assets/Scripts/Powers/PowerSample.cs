using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerSample", menuName = "PlayerAbilities/PowerSample")]
/// <summary>
/// The sample to follow to create a new Power
/// </summary>
public class PowerSample : Power
{
    public float range;
    public float stunTime;

    public override IEnumerator Use()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Enemy"));
        List<Collider2D> colliders = new List<Collider2D>();
        Physics2D.OverlapCircle(GameData.playerMovement.transform.position, range, filter, colliders);
        if(colliders.Count > 0)
        {
            foreach(Collider2D collider in colliders)
            {
                collider.GetComponent<EnemyHandler>().SetEffect(Effect.Stun, stunTime, true);
            }
        }

        yield return new WaitForFixedUpdate();
        yield return new WaitForSeconds(0.2f);
    }
}
