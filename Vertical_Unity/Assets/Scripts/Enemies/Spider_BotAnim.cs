using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider_BotAnim : MonoBehaviour
{
    public float jumpingTime;

    private Animator animator;
    private Spider_Bot spiderBot;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        spiderBot = GetComponent<Spider_Bot>();
    }

    void Update()
    {
        animator.SetBool("InTheAir", spiderBot.isJumping);

        animator.SetBool("IsCharging", spiderBot.jumpShotDelayRemaining < spiderBot.jumpShotDelay ? true : false);

        animator.SetBool("IsJumping", spiderBot.jumpShotDelayRemaining < jumpingTime ? true : false);
    }
}
