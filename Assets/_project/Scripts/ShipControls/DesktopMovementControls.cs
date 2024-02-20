using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class DesktopMovementControls : MovementControlsBase
{
    [SerializeField] float _deadZoneRadius = 0.1f;
    float _rollAmount = 0;

    Vector2 ScreenCenter => new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

    public override float YawAmount
    {
        get
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            float yaw = (mousePosition.x - ScreenCenter.x) / ScreenCenter.x;
            return Mathf.Abs(yaw) > _deadZoneRadius ? yaw : 0f;
        }
    }

    public override float PitchAmount
    {
        get
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            float pitch = (mousePosition.y - ScreenCenter.y) / ScreenCenter.y;
            return Mathf.Abs(pitch) > _deadZoneRadius ? pitch * -1: 0f;
        } 
        
    }

    public override float RollAmount
    {
        get
        {
            float roll;
            if (Keyboard.current.qKey.isPressed)
            {
                roll =  1f;
            }
            else
            {
                roll = Keyboard.current.eKey.isPressed ? -1f : 0f;
            }

            _rollAmount = Mathf.Lerp(_rollAmount, roll, Time.deltaTime * 3f);
            return _rollAmount;
        } 
        
    }

    public override float ThrustAmount => 
        Keyboard.current.wKey.isPressed ? 1f : (Keyboard.current.sKey.isPressed ? -1f : 0f);
}