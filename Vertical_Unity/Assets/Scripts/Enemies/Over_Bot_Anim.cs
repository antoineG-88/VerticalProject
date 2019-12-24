using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Over_Bot_Anim : MonoBehaviour
{
    Animator anim;
    private OverBot overBot;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        overBot = GetComponent<OverBot>();
    }
    void Update()
    {
        if (overBot.timebeforeRushAttack > 0 && overBot.isAtRange && overBot.rushAttackCooldownRemaining <= 0)
        {
            anim.SetBool("RushAttack", true);
        }
        else
        {
            anim.SetBool("RushAttack", false);
        }
    }
}
