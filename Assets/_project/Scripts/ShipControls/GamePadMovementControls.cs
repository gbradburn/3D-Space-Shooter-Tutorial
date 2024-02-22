using System;
using UnityEngine.InputSystem;

[Serializable]
public class GamePadMovementControls : MovementControlsBase
{
    public override float YawAmount => Gamepad.current.leftStick.x.ReadValue();
    public override float PitchAmount => Gamepad.current.leftStick.y.ReadValue();
    public override float RollAmount => Gamepad.current.rightStick.x.ReadValue() * -1f;
    public override float ThrustAmount => Gamepad.current.rightStick.y.ReadValue();
}