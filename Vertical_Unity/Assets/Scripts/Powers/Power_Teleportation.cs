using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerTeleportation", menuName = "PlayerAbilities/PowerTeleportation")]
/// <summary>
/// The sample to follow to create a new Power
/// </summary>
public class PowerTeleportation : Power
{
    public float range;
    public float stunTime;
    public int timewait;
    private Vector2 initialPosition;
    public GameObject TpPoint;
    private GameObject CurrentTpPoint;

    public override IEnumerator Use()
    {
        initialPosition = GameData.playerMovement.transform.position;
        CurrentTpPoint = Instantiate(TpPoint, GameData.playerMovement.transform.position,Quaternion.identity);
        yield return new WaitForSeconds(timewait);
        GameData.playerMovement.transform.position = initialPosition;
        Destroy(CurrentTpPoint);

    }
}

