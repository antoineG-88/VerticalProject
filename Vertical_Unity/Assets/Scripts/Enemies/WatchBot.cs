using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchBot : EnemyHandler
{
    [Header("WatchBot settings")]
    public float acceleration;
    public float maxSpeed;

    private void Start()
    {
        HandlerStart();

        targetPathfindingPosition = GameData.playerMovement.gameObject.transform.position;
    }

    private void Update()
    {
        HandlerUpdate();
    }

    private void FixedUpdate()
    {
        HandlerFixedUpdate();

        UpdateMovement();
    }

    public override void UpdateMovement()
    {
        if (!Is(Effect.Stun) && !Is(Effect.NoControl) && !Is(Effect.NoGravity))
        {
            targetPathfindingPosition = GameData.playerMovement.gameObject.transform.position;

            if (path != null && !pathEndReached)
            {
                Vector2 addedVelocity = new Vector2(pathDirection.x * acceleration, pathDirection.y * acceleration);

                rb.velocity += addedVelocity * Time.fixedDeltaTime;

                if (addedVelocity.magnitude > maxSpeed * slowEffectScale)
                {
                    rb.velocity = rb.velocity.normalized * maxSpeed * slowEffectScale;
                }
            }

            transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, Vector2.SignedAngle(Vector2.right, pathDirection)));
        }
    }

    public override bool TestCounter()
    {
        return false;
    }
}
