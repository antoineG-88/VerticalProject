using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnnemyHandler
{
    public string name;
    public float maxHealth;

    public float currentHealth;

    public abstract void TakeDamage(float damage, Vector2 knockBack, float invulnerableTime);
}
