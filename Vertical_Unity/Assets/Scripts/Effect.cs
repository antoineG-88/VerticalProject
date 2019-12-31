using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Effect
{
    public string effectName;
    public GameObject effectFX;
    [HideInInspector] public int index;

    public static Effect Stun
    {
        get
        {
            foreach (Effect effect in GameData.gameController.enemyEffects)
            {
                if (effect.effectName == "Stun")
                {
                    return effect;
                }
            }
            return null;
        }
    }

    public static Effect NoControl
    {
        get
        {
            foreach (Effect effect in GameData.gameController.enemyEffects)
            {
                if (effect.effectName == "NoControl")
                {
                    return effect;
                }
            }
            return null;
        }
    }

    public static Effect Hack
    {
        get
        {
            foreach (Effect effect in GameData.gameController.enemyEffects)
            {
                if (effect.effectName == "Hack")
                {
                    return effect;
                }
            }
            return null;
        }
    }

    public static Effect NoGravity
    {
        get
        {
            foreach (Effect effect in GameData.gameController.enemyEffects)
            {
                if (effect.effectName == "NoGravity")
                {
                    return effect;
                }
            }
            return null;
        }
    }

    public static Effect Slow
    {
        get
        {
            foreach (Effect effect in GameData.gameController.enemyEffects)
            {
                if (effect.effectName == "Slow")
                {
                    return effect;
                }
            }
            return null;
        }
    }

    public static Effect Magnetism
    {
        get
        {
            foreach (Effect effect in GameData.gameController.enemyEffects)
            {
                if (effect.effectName == "Magnetism")
                {
                    return effect;
                }
            }
            return null;
        }
    }

    public static Effect Immobilize
    {
        get
        {
            foreach (Effect effect in GameData.gameController.enemyEffects)
            {
                if (effect.effectName == "Immobilize")
                {
                    return effect;
                }
            }
            return null;
        }
    }
}
