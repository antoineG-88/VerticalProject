using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OS_Bot_Anim : MonoBehaviour
{
    Animator anim;
    private OS_Bot os_bot;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        os_bot = GetComponent<OS_Bot>();
    }
    void Update()
    {
        if (os_bot.snipAttackLockRange > 0 && os_bot.isAtRange && os_bot.snipAttackCooldownRemaining <= 0)
        {
            anim.SetBool("OS_Bot_Shot", true);
        }
        else
        {
            anim.SetBool("OS_Bot_Shot", false);
        }
    }
}
