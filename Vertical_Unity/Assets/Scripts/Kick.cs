using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Kick
{
    public string name;
    public int level;
    public Sprite icon;

    public float triggerDistance;

    public abstract void TriggerAttack(EnnemyHandler ennemy, PlayerMovement playerMovement, PlayerGrapplingHandler playerGrapplingHandler);
}
