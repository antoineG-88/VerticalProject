using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerBallClear", menuName = "PlayerAbilities/BallClear")]
/// <summary>
/// The sample to follow to create a new Power
/// </summary>
public class PowerBallClear : Power
{
    public float range;
    public GameObject sprite;
    public float size;
    public float clearTime;
    public float speed;

    public override IEnumerator Use()
    {
        Instantiate(sprite, GameData.playerMovement.transform.position, Quaternion.identity);
        Vector2 initialVelocity = GameData.playerManager.GetComponent<Rigidbody2D>().velocity;
        GameObject newObject = Instantiate(sprite, GameData.playerMovement.transform.position, Quaternion.identity);
        newObject.transform.localScale = new Vector2(size, size);
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Balls"));
        filter.useTriggers = true;
        List<Collider2D> ballscolliders = new List<Collider2D>();
        float timer = clearTime;
        while(timer > 0)
        {
            Physics2D.OverlapCircle(GameData.playerMovement.transform.position, range, filter, ballscolliders);
            GameData.playerMovement.Propel(initialVelocity.normalized * speed, true, true);
            if (ballscolliders.Count > 0)
            {
                foreach (Collider2D collider in ballscolliders)
                {
                    Destroy(collider.gameObject);
                }
            }
            timer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        GameData.playerMovement.Propel(initialVelocity, true, true);
    }
}
