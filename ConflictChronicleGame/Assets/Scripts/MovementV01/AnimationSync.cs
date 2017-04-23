using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSync : MonoBehaviour {
    #region <------ G L O B A L S  ------>
    //G L O B A L  R E F E R E N C E S
    public Animator animationsController;
    public Puppet2D_GlobalControl globalControlScript;
    public MoveScriptCharacterController charMovementScript;
    public PlayerActionController actions;
    //P U B L I C
    public float noMoveThreshold = 0.1f;
    public float returnToIdleTime = 0.2f;
    public float landingTime = 0.3f;
    [HideInInspector]
    public bool flipped;
    //P R I V A T E 
    private float slideWeight = 0.0f;
    private float combatWeight = 0.0f;
    private float lastFall = 0f;
    private bool jumped = false;

    private float attackFlipLeeway = 0.2f;

    #endregion	
	void Update () {
        globalControlScript.flip = flipped;
        Vector3 charVelocity = charMovementScript.controller.velocity;
        //J u m p i n g
        if (charMovementScript.currentJumpMode == JumpMode.grounded) {
            animationsController.SetBool("Falling", false);
            animationsController.SetFloat("MoveSpeed", charVelocity.magnitude / charMovementScript.moveSpeed, 0.1f, Time.deltaTime);
            if (Time.time > lastFall + landingTime) {
                animationsController.SetLayerWeight(3, 0f);
            }
            jumped = false;
            animationsController.SetFloat("SpeedMult", 1);
        } else if (charMovementScript.currentJumpMode == JumpMode.jumpstart) {
            if (!jumped) {
                animationsController.SetTrigger("Jump");
                jumped = true;
            }
            animationsController.SetLayerWeight(3, 0.8f);
        } else if (charMovementScript.currentJumpMode == JumpMode.fall) {
            lastFall = Time.time;
            animationsController.SetBool("Falling", true);
            animationsController.SetFloat("SpeedMult", 0.1f);
        } else if (charMovementScript.currentJumpMode == JumpMode.jump) {
            animationsController.SetFloat("SpeedMult", 0.1f);
        }

        //M o v i n g
        facingDirections(charMovementScript.inputDirection);
        if (charMovementScript.currentMoveMode == MoveMode.sprint && charMovementScript.controller.isGrounded) {
            animationsController.SetFloat("SpeedMult", charVelocity.magnitude / charMovementScript.sprintSpeed);
        }
        if (charMovementScript.currentMoveMode == MoveMode.slide) {
            slideWeight = Mathf.Lerp(slideWeight, 0.7f, 0.25f);
            animationsController.SetBool("Sliding", true);
            animationsController.SetLayerWeight(2, slideWeight);
        }
        if(charMovementScript.currentMoveMode == MoveMode.slowdown) {
            slideWeight = Mathf.Lerp(slideWeight, 0.3f, 0.25f);
            animationsController.SetBool("Sliding", true);
            animationsController.SetLayerWeight(2, slideWeight);
            animationsController.SetFloat("MoveSpeed", charVelocity.magnitude / charMovementScript.sprintSpeed);
        }
        if((charMovementScript.currentMoveMode == MoveMode.guard && charMovementScript.controller.isGrounded 
            && charMovementScript.currentJumpMode != JumpMode.jumpstart) 
            || (charMovementScript.currentMoveMode == MoveMode.slowdown && actions.ActionInProgress)) {
            combatWeight = Mathf.Lerp(combatWeight, 1f, 0.25f);
            animationsController.SetLayerWeight(4, combatWeight);
            float strafeDirection = (flipped) ? -charVelocity.x : charVelocity.x;
            strafeDirection *= 1.3f;
            animationsController.SetFloat("GuardMovement", Mathf.Lerp(animationsController.GetFloat("GuardMovement"), strafeDirection, 0.25f));
        }else if(charMovementScript.currentMoveMode != MoveMode.dodge) {
            combatWeight = Mathf.Lerp(combatWeight, 0f, 0.1f);
            animationsController.SetLayerWeight(4, combatWeight);
        }
        if(charMovementScript.currentMoveMode == MoveMode.dodge) {
            if (charMovementScript.dodgeVector != Vector3.zero && charMovementScript.lastDodgePress - Time.time == 0) {
                animationsController.SetLayerWeight(4, 1);
                if((flipped && charMovementScript.dodgeVector.x > 0) || (!flipped && charMovementScript.dodgeVector.x < 0)) {
                    animationsController.SetTrigger("DodgeBack");
                } else {
                    animationsController.SetTrigger("DodgeForward");
                }
            }
        }
        if (charMovementScript.currentMoveMode == MoveMode.normal || charMovementScript.currentMoveMode == MoveMode.sprint || charMovementScript.currentMoveMode == MoveMode.guard) {
            slideWeight = Mathf.Lerp(slideWeight, 0.0f, 0.3f);
            animationsController.SetBool("Sliding", false);
            animationsController.SetLayerWeight(2, slideWeight);
        }

        
    }

    public void facingDirections(Vector2 inVelocity) {
        //flipping
        if ((charMovementScript.currentMoveMode == MoveMode.normal || charMovementScript.currentMoveMode == MoveMode.sprint) ||
        (actions.playerAction.CurrentAction.InputType == InputType.FirstWeaponHold && actions.playerAction.TimeSinceHoldStart > 0 && actions.playerAction.TimeSinceHoldStart < attackFlipLeeway) ||
        (actions.playerAction.CurrentAction.InputType == InputType.FirstWeaponTap && actions.playerAction.CurrentAction.Modifier != Modifier.None)) {
            if (inVelocity.x < 0) {
                flipped = true;
            } 
            else if (inVelocity.x > 0) {
                flipped = false;
            }

            if ((inVelocity.x > 0.2 || inVelocity.x < -0.2) || (inVelocity.y > 0.7 || inVelocity.y < -0.7)) {
                animationsController.SetFloat("FacingFloat", inVelocity.y);
            }
        }
        if(charMovementScript.currentMoveMode == MoveMode.guard || charMovementScript.currentMoveMode == MoveMode.slowdown) {
            if ((inVelocity.x > 0.2 || inVelocity.x < -0.2) || (inVelocity.y > 0.7 || inVelocity.y < -0.7)) {
                animationsController.SetFloat("FacingFloat", inVelocity.y);
            }
        }



        //F a c i n g  D i r e c t i o n s
        //if(charMovementScript.currentMoveMode != MoveMode.guard && charMovementScript.currentMoveMode != MoveMode.dodge) {
        //    if (inVelocity.x < 0) {
        //        //globalControlScript.flip = inVelocity.x < 0;
        //        flipped = true;
        //    } else if (inVelocity.x > 0) {
        //        //globalControlScript.flip = false;
        //        flipped = false;
        //    }
        ////}
        //if ((inVelocity.x > 0.2 || inVelocity.x < -0.2) || (inVelocity.z > 0.7 || inVelocity.z < -0.7)) {
        //    animationsController.SetFloat("FacingFloat", inVelocity.z);
        //}
    }
}
