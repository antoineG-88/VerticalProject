using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerProjectil", menuName = "PlayerAbilities/PowerProjectil")]
/// <summary>
/// The sample to follow to create a new Power
/// </summary>
public class PowerProjectil : Power
{
    private Vector2 shootDirection;
    public GameObject projectilPrefab;
    public float lauchingSpeed;
    private GameObject projectil;
    public BoxCollider2D myProjectilCollider;
    public BoxCollider2D myProjectilCollider2;
    public float returnSpeed;
    private bool isInUse;
    public float timeDisapear;
    public float minDistance;
    public float slowTime;
    [Range(0.0f, 1.0f)] public float slowAmplitude;
    private Animator projectilAnimator;

    public override IEnumerator Use()
    {
        if (!isInUse)
        {
            isInUse = true;
            shootDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - GameData.playerMovement.transform.position);
            shootDirection.Normalize();
            //  parcouredDistance = GameData.playerMovement.transform.position* shootDirection * distance;
            projectil = Instantiate(projectilPrefab, GameData.playerMovement.transform.position, Quaternion.identity);
            Rigidbody2D rb = projectil.GetComponent<Rigidbody2D>();
            projectilAnimator = projectil.GetComponent<Animator>();
            rb.velocity = shootDirection * lauchingSpeed;
            projectil.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.up, rb.velocity));

            yield return new WaitForEndOfFrame();

            while (!Physics2D.OverlapBox(projectil.transform.position, myProjectilCollider.size, projectil.transform.rotation.eulerAngles.z, LayerMask.GetMask("Ground", "Enemy")))
            {
                yield return new WaitForFixedUpdate();
            }

            rb.velocity = Vector2.zero;
            projectilAnimator.SetBool("Transforming", true);
            float timer = timeDisapear;

            while (!GameData.gameController.input.leftTriggerDown && timer > 0)
            {
                yield return new WaitForFixedUpdate();
                timer -= Time.fixedDeltaTime;
            }

            if (timer <= 0)
            {
                Destroy(projectil);
            }
            else
            {
                projectilAnimator.SetBool("Transforming", false);
                while (Vector2.Distance(GameData.playerMovement.transform.position, projectil.transform.position) >= minDistance)
                {
                    projectil.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Vector2.SignedAngle(Vector2.down, rb.velocity));
                    ContactFilter2D filter = new ContactFilter2D();
                    filter.SetLayerMask(LayerMask.GetMask("Enemy"));
                    List<Collider2D> enemyColliders = new List<Collider2D>();
                    Physics2D.OverlapBox(projectil.transform.position, myProjectilCollider2.size, projectil.transform.rotation.eulerAngles.z, filter, enemyColliders);

                    if(enemyColliders.Count > 0)
                    {
                        foreach(Collider2D enemyCollider in enemyColliders)
                        {
                            enemyCollider.GetComponent<EnemyHandler>().SetSlowEffect(slowTime, false, slowAmplitude);
                        }
                    }
                    shootDirection = GameData.playerMovement.transform.position - projectil.transform.position;
                    projectil.GetComponent<Rigidbody2D>().velocity = shootDirection.normalized * returnSpeed;
                    yield return new WaitForFixedUpdate();
                }
                Destroy(projectil);
            }

            isInUse = false;

        }

    }

}