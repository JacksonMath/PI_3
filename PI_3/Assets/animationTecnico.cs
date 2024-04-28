using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class animationTecnico : MonoBehaviour

{
    public LedgeGrabbing grabbing;
    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (grabbing.holding)
        {
            anim.SetBool("Climb", true);
        }
        else
        {
            anim.SetBool("Climb", false);
        }
    }
}
