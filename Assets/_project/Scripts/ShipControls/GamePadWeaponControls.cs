using System;
using UnityEngine.InputSystem;

[Serializable]
public class GamePadWeaponControls : WeaponControlsBase
{
    public override bool PrimaryFired => Gamepad.current.rightTrigger.isPressed;
    public override bool SecondaryFired => Gamepad.current.leftTrigger.isPressed;
}