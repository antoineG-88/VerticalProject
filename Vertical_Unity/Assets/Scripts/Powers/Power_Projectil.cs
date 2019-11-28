using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerProjectil", menuName = "PlayerAbilities/PowerProjectil")]
/// <summary>
/// The sample to follow to create a new Power
/// </summary>
public class PowerProjectil : Power
{
    public float range;
    public float stunTime;
    private Vector2 shootDirection;
    public GameObject projectilPrefab;
    public float distance;
    private GameObject projectil;

    public override IEnumerator Use()
    {
        shootDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - GameData.playerMovement.transform.position);
        shootDirection.Normalize();
      //  parcouredDistance = GameData.playerMovement.transform.position* shootDirection * distance;
        projectil = Instantiate(projectilPrefab, GameData.playerMovement.transform.position, Quaternion.identity);
        projectil.GetComponent<Rigidbody2D>().velocity = shootDirection * distance;

         void OnTriggerEnter2D(Collider2D collider) {
            if(collider.CompareTag("Ground") || collider.CompareTag("Enemy")){
                projectil.GetComponent<Rigidbody2D>().velocity = zero;

            }

        }
            yield return new WaitForEndOfFrame();
    }
}