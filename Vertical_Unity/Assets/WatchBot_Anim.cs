using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchBot_Anim : MonoBehaviour
{
    Animator anim;
    private WatchBot watchBot;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        watchBot = GetComponent<WatchBot>();
    }
    void Update()
   {
        if (watchBot.timebeforeRangeAttack > 0 && watchBot.isAtRange && watchBot.rangeAttackCooldownRemaining <= 0)    
        {
            anim.SetBool("WatchBot_Shot", true);
        }
        else
        {
            anim.SetBool("WatchBot_Shot", false);
        }
   }
}
