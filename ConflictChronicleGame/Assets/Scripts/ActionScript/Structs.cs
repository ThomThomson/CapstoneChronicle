using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveMode { normal, sprint, slowdown, slide, guard, dodge, stagger };

public enum JumpMode { grounded, jumpstart, jump, fall };

public enum InputType { None, FirstWeaponTap, FirstWeaponHold, firstWeaponRelease, SecondWeaponTap, SecondWeaponHold, secondWeaponRelease, Alternate }

public enum Modifier { None, Tilt, Power }

public enum Direction { None, Up, Right, Left, Down }

public struct Action {
    public Action(InputType inputType, Modifier modifier, Direction direction) {
        InputType = inputType;
        Modifier = modifier;
        Direction = direction;
    }
    public InputType InputType;
    public Modifier Modifier;
    public Direction Direction;
}
