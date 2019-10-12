using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Kick : ScriptableObject
{
    public new string name;
    public int level;
    public Sprite icon;

    public float triggerDistance;

    public abstract void Use(EnnemyHandler ennemy);
}
