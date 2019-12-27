using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerTrainee", menuName = "PlayerAbilities/Trainee")]
/// <summary>
/// The sample to follow to create a new Power
/// </summary>
public class PowerTrainee : Power
{
    public float range;
    public float stunTime;
    public GameObject PrefabTrainee;
    public float waitingTime;
    public float powerTime;
    private float timeBetweenInst;
    private List<GameObject> effectposition = new List<GameObject>();
    public float size;

    public override IEnumerator Use()
    {

        float powerTimer = 0;
        timeBetweenInst = 0;
        while (powerTimer <= powerTime)
        {
            GameObject Trainee = new GameObject(); ;
            if (timeBetweenInst <= 0)
            {
                Trainee = Instantiate(PrefabTrainee, GameData.playerMovement.transform.position, Quaternion.identity);
                Trainee.transform.localScale = new Vector2(size, size);
                effectposition.Add(Trainee);
                timeBetweenInst = waitingTime;
            }
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Enemy"));
            List<Collider2D> EnemyColliders = new List<Collider2D>();

            foreach (GameObject trainee in effectposition)
            {


                Physics2D.OverlapCircle(Trainee.transform.position, range, filter, EnemyColliders);
                if (EnemyColliders.Count > 0)
                {
                    foreach (Collider2D collider in EnemyColliders)
                    {
                        collider.GetComponent<EnemyHandler>().SetEffect(Effect.Stun, stunTime, true);

                    }
                }
            }
            powerTimer += Time.fixedDeltaTime;
            timeBetweenInst -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        foreach (GameObject Trainee in effectposition)
        {
            Destroy(Trainee);
        }
        effectposition.Clear();
    }
}
