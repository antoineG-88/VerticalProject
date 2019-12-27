using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Power : ScriptableObject
{
    public string powerName;
    [TextArea] public string description;
    public Sprite icon;

    public float cooldown;

    public abstract IEnumerator Use();
}
