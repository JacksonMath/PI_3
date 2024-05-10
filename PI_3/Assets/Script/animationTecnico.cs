using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class animationTecnico : MonoBehaviour

{
    public LedgeGrabbing grabbing;
    public LedgeGrabbingDone grabbingDone;
    public Climbing climbing;
    public ClimbingDone climbingDone;
    public PlayerMovementAdvanced playerMov;
    public Animator anim;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (playerMov.grounded)
        {
            anim.SetBool("Jump", false);
            anim.SetFloat("Speed", rb.velocity.magnitude);
        }

        //Anima��o de agarrar na parede
        if (grabbing.holding || grabbingDone.holding)
        {
            anim.SetBool("Grab", true);
        }
        else
        {
            anim.SetBool("Grab", false);
        }

        //Anima��o de escalar na parede
        if (climbing.climbing || climbingDone.climbing)
        {
            anim.SetBool("Climb", true);
        }
        else
        {
            anim.SetBool("Climb", false);
        }

        //Anima��o de fall
        if(playerMov.state == PlayerMovementAdvanced.MovementState.air && !playerMov.grounded)
        {
            anim.SetBool("Air", true);
        }
        else
        {
            anim.SetBool("Air", false);
        }

        //Anima��o de WallJump
        if(climbing.exitingWall)
        {
            anim.SetBool("WallJump", true);
        }
        else
        {
            anim.SetBool("WallJump", false);
        }

        //Anima��o de cair no ch�o
        if (playerMov.lastState == PlayerMovementAdvanced.MovementState.air && playerMov.grounded)
        {
            anim.SetTrigger("Floor");
            anim.SetBool("Jump", false);
        }

        //Anima��o de Pulo do ch�o
        if (/*playerMov.grounded && Input.GetKeyUp(playerMov.jumpKey)*/ playerMov.isJumping)
        {
            anim.SetBool("Jump", true);
        }
    }
}
