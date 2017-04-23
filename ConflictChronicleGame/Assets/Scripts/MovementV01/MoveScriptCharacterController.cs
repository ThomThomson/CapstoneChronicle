using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveScriptCharacterController : MonoBehaviour {
    #region <------ G L O B A L S  ------>
    //G L O B A L  references
    public CharacterController controller;
    public PlayerActionController actions;
    [HideInInspector]
    public Vector2 inputDirection = Vector2.zero;
    public Vector3 currentVelocity = Vector3.zero;
    public MoveMode currentMoveMode;
    public JumpMode currentJumpMode;
    [HideInInspector]
    public float lastActionTime = 0f;
    [HideInInspector]
    public Vector3 dodgeVector = Vector3.zero;

    private Vector3 staggerVector = Vector3.zero;
    private float staggerSpeed;
    private float staggerTime = -1;
    private float staggerLength = -1;
    private bool staggering;

    [HideInInspector]
    public bool canMove = true;
    //P U B L I C movement variables
    public float gravity = 20.0f;
    public float moveSpeed = 4.0f;
    public float sprintDampening = 0.3f;
    public float sprintSpeed = 0f;
    public float dodgeSpeed = 12f;
    public float slideThreshold = 1f;
    public float inAirControl = 0.1f;
    public float jumpHeight = 8f;
    public float jumpDelay = 0.3f;
    public float timeToStopGuard = 5f;
    public float guardSpeed = 3f;
    //P R I V A T E movement variables
    private float lastSprintPress = 0f;
    private float sprintCooldown = 1f;
    private float dodgeCooldown = 0.6f;
    private float dodgeLength = 0.1f;
    private float dodgeMoveCooldown = 0.1f;
    private float jumpCoolDown = 0.2f;
    private float lastFallingTime = 0.0f;
    [HideInInspector]
    public float lastJumpPress;
    [HideInInspector]
    public float lastDodgePress = 0f;

    public string Jump;
    public string HorizontalAxis;
    public string VerticalAxis;
    public string Sprint;

    #endregion

    #region <------ U N I T Y  M E T H O D S  ------>
    void Start() {
        controller = GetComponent<CharacterController>();
        actions = GetComponent<PlayerActionController>();
        currentMoveMode = MoveMode.normal;
        currentJumpMode = JumpMode.grounded;
    }

    void Update () {
        canMove = canCharMove();
        inputDirection = new Vector2(Input.GetAxisRaw(HorizontalAxis), Input.GetAxisRaw(VerticalAxis));
        if (controller.isGrounded) {
            currentJumpMode = (currentJumpMode == JumpMode.jumpstart) ? JumpMode.jumpstart: JumpMode.grounded;
            currentMoveMode = moveModeFromInput(inputDirection);
            currentVelocity = velocityFromMoveMode(inputDirection);
            currentVelocity = transform.TransformDirection(currentVelocity);
            if (Input.GetButton(Jump)) {
                if (currentJumpMode != JumpMode.jumpstart && Time.time > lastFallingTime + jumpCoolDown && lastFallingTime != 0) {
                    lastJumpPress = Time.time;
                    currentJumpMode = JumpMode.jumpstart;
                }
            }
            currentVelocity.y -= gravity * Time.deltaTime;
        }else {
            currentVelocity.y -= gravity * Time.deltaTime;
            currentJumpMode = (currentVelocity.y > jumpHeight / 2) ? JumpMode.jump : JumpMode.fall;
        }
        if(currentJumpMode == JumpMode.jumpstart && Time.time > lastJumpPress + jumpDelay) {
            currentVelocity.y = jumpHeight;
        }
        if(currentJumpMode == JumpMode.fall) {
            lastFallingTime = Time.time;
        }
        controller.Move(currentVelocity * Time.deltaTime);
    }//E N D  M E T H O D Update
    #endregion

    #region <------ H E L P E R  M E T H O D S  ------>
    private MoveMode moveModeFromInput(Vector2 input) {
        MoveMode returnMode = MoveMode.normal;
        if (Input.GetButton(Sprint)) {
            if (actions.blocking && Time.time > lastDodgePress + dodgeCooldown && currentMoveMode == MoveMode.guard &&
                Time.time > lastSprintPress + sprintCooldown && Mathf.Abs(inputDirection.x) + Mathf.Abs(inputDirection.y) / 2 > 0.5) {
                returnMode = MoveMode.dodge;
                lastDodgePress = Time.time;
            } else if (Mathf.Abs(inputDirection.x) + Mathf.Abs(inputDirection.y) / 2 < 0.5 && Time.time > lastDodgePress
                && currentMoveMode == MoveMode.guard) {
                returnMode = MoveMode.normal;
                lastActionTime = 0;
                lastDodgePress = Time.time;
            } else if (actions.ActionInProgress) {
                returnMode = MoveMode.slowdown;
            } else if (!actions.blocking) {
                returnMode = MoveMode.sprint;
                lastSprintPress = Time.time;
            } else { returnMode = MoveMode.guard; }//if you're pressing sprint and blocking at the same time, you're just waiting to do the next dodge.
        } else if (lastSprintPress != 0 && Time.time < lastSprintPress + sprintCooldown && (currentVelocity.magnitude > sprintSpeed / 2)) {
            returnMode = MoveMode.slowdown;
        } else if(lastActionTime != 0 && (Time.time < lastActionTime + timeToStopGuard)) {
            returnMode = MoveMode.guard;
        }
        if(currentMoveMode == MoveMode.sprint || currentMoveMode == MoveMode.slide) {
            if (Mathf.Abs(input.y - (currentVelocity.z) / sprintSpeed) > slideThreshold
             || Mathf.Abs(input.x - (currentVelocity.x) / sprintSpeed) > slideThreshold) {
                returnMode = MoveMode.slide;
            }
        }
        if (currentMoveMode == MoveMode.dodge && Time.time < lastDodgePress + dodgeLength) {
            returnMode = MoveMode.dodge;
        } else { dodgeVector = Vector3.zero; }
        if (currentMoveMode == MoveMode.stagger && Time.time < staggerTime + staggerLength) {
                returnMode = MoveMode.stagger;
        }
        Debug.Log(returnMode + this.name);
        return returnMode;
    }//E N D  M E T H O D MoveModeFromInput

    private Vector3 velocityFromMoveMode(Vector2 input) {
        if(currentMoveMode == MoveMode.stagger) {
            return staggerVector;
        }
        if (canMove) {
            if (currentMoveMode == MoveMode.sprint || currentMoveMode == MoveMode.slide) {
                float deltaX = sprintDampening * input.x;
                float deltaZ = sprintDampening * input.y;
                float newVelX = (Mathf.Abs(currentVelocity.x + deltaX) <= sprintSpeed) ? currentVelocity.x + deltaX : currentVelocity.x;
                float newVelZ = (Mathf.Abs(currentVelocity.z + deltaZ) <= sprintSpeed) ? currentVelocity.z + deltaZ : currentVelocity.z;
                return new Vector3(newVelX, 0, newVelZ);
            } else if (currentMoveMode == MoveMode.slowdown) {
                Vector3 returnVector = new Vector3(input.x, 0, input.y);
                returnVector *= moveSpeed;
                return Vector3.Lerp(currentVelocity, returnVector, 0.03f);
            } else if (currentMoveMode == MoveMode.guard) {
                Vector3 returnVector = new Vector3(input.x, 0, input.y);
                returnVector *= guardSpeed;
                return returnVector;
            } else if (currentMoveMode == MoveMode.dodge) {
                if (dodgeVector == Vector3.zero) {
                    float dodgex = (input.x > 0) ? 1f : -1f; float dodgez = (input.y > 0) ? 1f : -1f;
                    dodgex = (input.x == 0) ? 0 : dodgex; dodgez = (input.y == 0) ? 0 : dodgez;
                    dodgeVector = new Vector3(dodgex, 0, dodgez);
                    dodgeVector *= dodgeSpeed;
                }
                return dodgeVector;
            } else {
                Vector3 returnVector = new Vector3(input.x, 0, input.y);
                returnVector *= moveSpeed;
                return returnVector;
            }
        }else {
            return Vector3.zero;
        }
        
    }//E N D  M E T H O D velocityFromMoveMode

    private bool canCharMove() {
        return ((Time.time > lastDodgePress + dodgeLength + dodgeMoveCooldown || currentMoveMode == MoveMode.dodge) &&
            !actions.attackStopsMovement);
    }

    public void Stagger(float staggerLength, float staggerSpeed, Vector3 hitFrom) {
        if (Time.time > this.staggerTime + this.staggerLength || (this.staggerTime < 0 && this.staggerLength < 0)) {
            currentMoveMode = MoveMode.stagger;
            staggerTime = Time.time;
            this.staggerLength = staggerLength;
            this.staggerSpeed = staggerSpeed;
            float staggerX = (hitFrom.x > this.transform.position.x) ? -1f : 1f;
            float staggerZ = (hitFrom.z > this.transform.position.z) ? -1f : 1f;
            staggerX = (hitFrom.x == this.transform.position.x) ? 0 : staggerX;
            staggerZ = (hitFrom.z == this.transform.position.z) ? 0 : staggerZ;
            staggerVector = new Vector3(staggerX, 0, staggerZ);
            staggerVector *= staggerSpeed;
        }
    }

    public Vector3 getVelocity() {return currentVelocity;}
    #endregion
}
