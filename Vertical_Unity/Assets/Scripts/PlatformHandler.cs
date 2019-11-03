using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformHandler : MonoBehaviour
{
    public List<PlatformConnection> connections;
    public List<Collider2D> detectionColliders;

    void Start()
    {
        foreach(PlatformConnection connection in connections)
        {
            connection.attachedPlatformHandler = this;
        }
    }

    public bool IsUnder(GameObject target)
    {
        bool isUnder = false;

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.NoFilter();
        List<Collider2D> colliders = new List<Collider2D>();

        foreach(Collider2D detectionCollider in detectionColliders)
        {
            detectionCollider.OverlapCollider(contactFilter, colliders);

            foreach (Collider2D collider in colliders)
            {
                if (collider.gameObject == target)
                {
                    isUnder = true;
                }
            }
        }

        return isUnder;
    }
}
