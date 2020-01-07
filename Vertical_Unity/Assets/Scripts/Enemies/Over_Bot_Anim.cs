using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Over_Bot_Anim : MonoBehaviour
{
    Animator anim;
    private OverBot overBot;

    private void Start()
    {
        anim = GetComponent<Animator>();
        overBot = GetComponent<OverBot>();
    }
    void Update()
    {
        anim.SetBool("IsRushing", overBot.isRushing);

        anim.SetBool("IsShooting", overBot.isShooting);
    }
}
