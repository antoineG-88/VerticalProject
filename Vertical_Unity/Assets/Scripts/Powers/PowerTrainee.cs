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
    public float timeBeforeCloudEffect;
    public float cloudLifeSpan;
    private List<GameObject> effectposition;
    private List<float> cloudsLifeSpend;
    public float size;
    public float smallSize;

    public override IEnumerator Use()
    {
        if(effectposition == null)
        {
            effectposition = new List<GameObject>();
            cloudsLifeSpend = new List<float>();
        }
        float powerTimer = 0;
        timeBetweenInst = 0;
        while (powerTimer <= powerTime)
        {
            if (timeBetweenInst <= 0)
            {
                GameObject Trainee = Instantiate(PrefabTrainee, GameData.playerMovement.transform.position, Quaternion.identity);
                Trainee.transform.localScale = new Vector2(smallSize, smallSize);
                cloudsLifeSpend.Add(0);
                effectposition.Add(Trainee);
                timeBetweenInst = waitingTime;
            }

            if(powerTimer == 0 && effectposition.Count > 0)
            {
                GameData.playerAttackManager.StartCloudsUpdate();
            }

            powerTimer += Time.fixedDeltaTime;
            timeBetweenInst -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    public IEnumerator UpdateClouds()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Enemy"));

        while (effectposition.Count > 0)
        {
            List<Collider2D> EnemyColliders = new List<Collider2D>();
            int limit = effectposition.Count;
            for (int i = 0; i < limit; i++)
            {
                cloudsLifeSpend[i] += Time.fixedDeltaTime;
                if (cloudsLifeSpend[i] <= cloudLifeSpan && cloudsLifeSpend[i] > timeBeforeCloudEffect)
                {
                    effectposition[i].transform.localScale = new Vector2(size, size);
                    Physics2D.OverlapCircle(effectposition[i].transform.position, range, filter, EnemyColliders);
                    if (EnemyColliders.Count > 0)
                    {
                        foreach (Collider2D collider in EnemyColliders)
                        {
                            collider.GetComponent<EnemyHandler>().SetEffect(Effect.Stun, stunTime, false);
                            collider.GetComponent<EnemyHandler>().SetEffect(Effect.Immobilize, stunTime, false);
                        }
                    }
                }
                else if(cloudsLifeSpend[i] > cloudLifeSpan)
                {
                    Destroy(effectposition[i]);
                    effectposition.RemoveAt(i);
                    cloudsLifeSpend.RemoveAt(i);
                    limit--;
                }
            }

            yield return new WaitForFixedUpdate();
        }

        effectposition.Clear();
        cloudsLifeSpend.Clear();
    }
}
