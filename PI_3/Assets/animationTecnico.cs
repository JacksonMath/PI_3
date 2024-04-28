using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;
using KinematicCharacterController.Walkthrough.AddingImpulses;
using KinematicCharacterController.Walkthrough.ChargingState;
using KinematicCharacterController.Walkthrough.ClimbingLadders;
using KinematicCharacterController.Walkthrough.WallJumping;
using Unity.VisualScripting;
using UnityEditor.Rendering;

public class animationTecnico : MonoBehaviour

{
    public KinematicCharacterController.Walkthrough.ClimbingLadders.MyCharacterController MyCharacterController;
    public KinematicCharacterController.Walkthrough.ClimbingLadders.CharacterState CharacterState;
    public KinematicCharacterController.Walkthrough.ClimbingLadders.ClimbingState ClimbingState;

    public KinematicCharacterMotor Motor;
    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ClimbingState != null)
        {
            anim.SetBool("Climb", true);
        }
    }
}
