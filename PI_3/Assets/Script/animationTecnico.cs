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

        //Animação de agarrar na parede
        if (grabbing.holding || grabbingDone.holding)
        {
            anim.SetBool("Grab", true);
        }
        else
        {
            anim.SetBool("Grab", false);
        }

        //Animação de escalar na parede
        if (climbing.climbing || climbingDone.climbing)
        {
            anim.SetBool("Climb", true);
        }
        else
        {
            anim.SetBool("Climb", false);
        }

        //Animação de fall
        if(playerMov.state == PlayerMovementAdvanced.MovementState.air && !playerMov.grounded)
        {
            anim.SetBool("Air", true);
        }
        else
        {
            anim.SetBool("Air", false);
        }

        //Animação de WallJump
        if(climbing.exitingWall)
        {
            anim.SetBool("WallJump", true);
        }
        else
        {
            anim.SetBool("WallJump", false);
        }

        //Animação de cair no chão
        if (playerMov.lastState == PlayerMovementAdvanced.MovementState.air && playerMov.grounded)
        {
            anim.SetTrigger("Floor");
            anim.SetBool("Jump", false);
        }

        //Animação de Pulo do chão
        if (/*playerMov.grounded && Input.GetKeyUp(playerMov.jumpKey)*/ playerMov.isJumping)
        {
            anim.SetBool("Jump", true);
        }
    }
}
