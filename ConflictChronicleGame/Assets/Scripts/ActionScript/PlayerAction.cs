using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAction : MonoBehaviour {
    public Action CurrentAction;

    public string FirstWeapon;
    public string Alternate1;
    public string Alternate2;
    public string HorizontalAxis;
    public string VerticalAxis;


    private float powerThreshold = 0.9f;
    private float tiltThreshold = 0.7f;
    private float timeToTriggerHold = 0.2f;

    private float minPowerChargeTime = 0.5f;
    private float maxPowerChargeTime = 2f;

    public float TimeSinceHoldStart;
    private float timeHoldingAttack;
    private bool holding;
	
	void Update () {
        if (Input.GetButton(FirstWeapon)) {
            timeHoldingAttack += Time.deltaTime;
        }
        if(timeHoldingAttack > timeToTriggerHold) {
            timeHoldingAttack = 0;
            holding = true;
        }
        CurrentAction = new Action(GetInputType(), GetModifier(), GetDirection());
        //Debug.Log(holding + " --- " + timeSinceHoldStart + " --- " + CurrentAction.InputType);
    }

    private InputType GetInputType() {
        if (holding) {
            TimeSinceHoldStart += Time.deltaTime;
            if ((!Input.GetButton(FirstWeapon) && TimeSinceHoldStart > minPowerChargeTime) ||
                TimeSinceHoldStart > maxPowerChargeTime) {
                holding = false;
                return InputType.firstWeaponRelease;
            }
            return InputType.FirstWeaponHold;
        }

        if (Input.GetButtonUp(FirstWeapon)) {
            timeHoldingAttack = 0;
            return InputType.FirstWeaponTap;
        }
        if (Input.GetAxis(Alternate1) > 0.1 || Input.GetAxis(Alternate2) > 0.1) {
            return InputType.Alternate;
        }
        return InputType.None;
    }

    private Modifier GetModifier() {
        if (Mathf.Abs(Input.GetAxis(HorizontalAxis)) > powerThreshold || Mathf.Abs(Input.GetAxis(VerticalAxis)) > powerThreshold) { return Modifier.Power; } 
        else if (Mathf.Abs(Input.GetAxis(HorizontalAxis)) > tiltThreshold || Mathf.Abs(Input.GetAxis(VerticalAxis)) > tiltThreshold) { return Modifier.Tilt; } 
        else { return Modifier.None; }
    }

    private Direction GetDirection() {
        Direction strongestDirection = Direction.None;
        float strongestDirectionStrength = 0f;
        float currentDirectionStrength = 0f;
        if (Input.GetAxis(HorizontalAxis) > 0) {
            currentDirectionStrength = Mathf.Abs(Input.GetAxis(HorizontalAxis));
            if (currentDirectionStrength > strongestDirectionStrength) {
                strongestDirection = Direction.Right;
                strongestDirectionStrength = currentDirectionStrength;
            }
        }
        if (Input.GetAxis(HorizontalAxis) < 0) {
            currentDirectionStrength = Mathf.Abs(Input.GetAxis(HorizontalAxis));
            if (currentDirectionStrength > strongestDirectionStrength) {
                strongestDirection = Direction.Left;
                strongestDirectionStrength = currentDirectionStrength;
            }
        }
        if (Input.GetAxis(VerticalAxis) > 0) {
            currentDirectionStrength = Mathf.Abs(Input.GetAxis(VerticalAxis));
            if (currentDirectionStrength > strongestDirectionStrength) {
                strongestDirection = Direction.Up;
                strongestDirectionStrength = currentDirectionStrength;
            }
        }
        if (Input.GetAxis(VerticalAxis) < 0) {
            currentDirectionStrength = Mathf.Abs(Input.GetAxis(VerticalAxis));
            if (currentDirectionStrength > strongestDirectionStrength) {
                strongestDirection = Direction.Down;
                strongestDirectionStrength = currentDirectionStrength;
            }
        }
        return strongestDirection;
    }

}
