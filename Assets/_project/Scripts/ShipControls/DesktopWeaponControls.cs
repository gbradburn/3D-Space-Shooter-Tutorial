using System;
using UnityEngine.InputSystem;

[Serializable]
public class DesktopWeaponControls : WeaponControlsBase
{
    public override bool PrimaryFired => Mouse.current.leftButton.isPressed;
    public override bool SecondaryFired => Mouse.current.rightButton.isPressed;
}