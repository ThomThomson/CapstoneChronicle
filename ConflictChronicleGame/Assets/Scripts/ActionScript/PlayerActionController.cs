using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionController : MonoBehaviour {
    [HideInInspector]
    public PlayerAction playerAction;
    
    private MoveScriptCharacterController movementScript;
    private AnimationSync animations;
    private float attackLengthTap = 0.3f;
    private float attackLength = 0.6f;
    private float attackLengthPWR = 0.8f;

    private int attackChain = 0;
    private float lastChainAttackTime = 0f;
    [HideInInspector]
    public int CurrentAttackDamage;

    public bool ActionInProgress { get; set; }
    public bool attackStopsMovement;
    public bool attackIsTwoWay;

    public Action currentAction;
    [HideInInspector]
    public bool blocking;

	void Start () {
        playerAction = gameObject.GetComponent<PlayerAction>();
        animations = gameObject.GetComponent<AnimationSync>();
        movementScript = gameObject.GetComponent<MoveScriptCharacterController>();
    }
	
	void Update () {
        currentAction = playerAction.CurrentAction;
        if (currentAction.InputType == InputType.FirstWeaponTap) {
            if (currentAction.Modifier == Modifier.Tilt || currentAction.Modifier == Modifier.Power) {
                if (currentAction.Direction == Direction.Right || currentAction.Direction == Direction.Left) {
                    Tilt("TiltSide");
                }
                else if (currentAction.Direction == Direction.Up) {
                    Tilt("TiltUp");
                }
                else if (currentAction.Direction == Direction.Down) {
                    Tilt("TiltDown");
                }
            } else {
                TapAttack();
                
            }
        }
        else if(currentAction.InputType == InputType.FirstWeaponHold) {
            if (currentAction.Direction == Direction.Right || currentAction.Direction == Direction.Left) {
                PowerAttackStart("PowerForward");
            } else if(currentAction.Direction == Direction.None) {
                attackIsTwoWay = true;
                PowerAttackStart("PowerNeutral");
            } else if (currentAction.Direction == Direction.Up) {
                PowerAttackStart("PowerUp");
            } else if (currentAction.Direction == Direction.Down) {
                attackIsTwoWay = true;
                PowerAttackStart("PowerDown");
            }
        }
        else if(currentAction.InputType == InputType.firstWeaponRelease) {
            PowerAttackRelease("PowerRelease");
        }

        //Blocking
        else if (currentAction.InputType == InputType.Alternate) {
            testBlock();
        } else {
            animations.animationsController.SetBool("Blocking", false);
            blocking = false;
        }
        //reset the attack chain
        if(Time.time - lastChainAttackTime > attackLength) {
            attackChain = 0;
        }

    }


    public void testBlock() {
        if (!ActionInProgress && !blocking) {
            blocking = true;
            animations.animationsController.SetTrigger("Block");
            animations.animationsController.SetBool("Blocking", true);
            movementScript.lastActionTime = Time.time;
        }else if (blocking) {
            movementScript.lastActionTime = Time.time;
        }
    }

    public void TapAttack() {
        if (!ActionInProgress) {
            blocking = false;
            switch (attackChain) {
            case 1:
                ActionInProgress = true;
                animations.animationsController.SetTrigger("TapAttack2");
                StartCoroutine("AttackAction", attackLengthTap / 2);
                lastChainAttackTime = Time.time;
                attackChain++;
                attackStopsMovement = true;
                CurrentAttackDamage = 40;
                break;
            case 2:
                ActionInProgress = true;
                animations.animationsController.SetTrigger("TapAttack3");
                animations.animationsController.SetTrigger("Yell");
                StartCoroutine("AttackAction", attackLengthTap);
                attackChain = 0;
                CurrentAttackDamage = 100;
                break;
            default:
                ActionInProgress = true;
                animations.animationsController.SetTrigger("TapAttack1");
                StartCoroutine("AttackAction", attackLengthTap / 2);
                lastChainAttackTime = Time.time;
                attackChain++;
                CurrentAttackDamage = 20;
                attackStopsMovement = true;
                break;
            }
            movementScript.lastActionTime = Time.time;
        }
        
    }

    public void Tilt(string tiltName) {
        blocking = false;
        if (!ActionInProgress) {
            ActionInProgress = true;
            StartCoroutine("AttackAction", attackLength);
            animations.animationsController.SetTrigger(tiltName);
            animations.animationsController.SetTrigger("Yell");
            attackStopsMovement = true;
            CurrentAttackDamage = 80;
            movementScript.lastActionTime = Time.time;
        }
    }

    public void PowerAttackStart(string pwrName) {
        blocking = false;
        if (!ActionInProgress) {
            animations.animationsController.ResetTrigger("PowerRelease");
            ActionInProgress = true;
            animations.animationsController.SetTrigger(pwrName);
            attackStopsMovement = true;
            movementScript.lastActionTime = Time.time;
        }
    }

    public void PowerAttackRelease(string pwrName) {
        blocking = false;
        StartCoroutine("AttackAction", attackLengthPWR);
        animations.animationsController.SetTrigger("Yell");
        animations.animationsController.SetTrigger(pwrName);
        CurrentAttackDamage = 150;
        movementScript.lastActionTime = Time.time;
    }

    IEnumerator AttackAction(float inTime) {
        yield return new WaitForSeconds(inTime);
        ActionInProgress = false;
        attackStopsMovement = false;
        attackIsTwoWay = false;
        playerAction.TimeSinceHoldStart = 0;
    }


}
