using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXScript : MonoBehaviour
{
    public float visualEffectTime;

    private float timeSpend;
    void Start()
    {
        timeSpend = 0;
    }
    void Update()
    {
        timeSpend += Time.deltaTime;
        if(timeSpend > visualEffectTime)
        {
            Destroy(gameObject);
        }
    }
}
