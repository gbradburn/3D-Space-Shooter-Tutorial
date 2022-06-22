using System;
using UnityEngine;

[Serializable]
public class DesktopMovementControls : MovementControlsBase
{
    [SerializeField] float _deadZoneRadius = 0.1f;
    
    Vector2 ScreenCenter => new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

    public override float YawAmount
    {
        get
        {
            Vector3 mousePosition = Input.mousePosition;
            float yaw = (mousePosition.x - ScreenCenter.x) / ScreenCenter.x;
            return Mathf.Abs(yaw) > _deadZoneRadius ? yaw : 0f;
        }
    }

    public override float PitchAmount
    {
        get
        {
            Vector3 mousePosition = Input.mousePosition;
            float pitch = (mousePosition.y - ScreenCenter.y) / ScreenCenter.y;
            return Mathf.Abs(pitch) > _deadZoneRadius ? pitch * -1: 0f;
        } 
        
    }

    public override float RollAmount
    {
        get
        {
            if (Input.GetKey(KeyCode.Q))
            {
                return 1f;
            }

            return Input.GetKey(KeyCode.E) ? -1f : 0f;
        } 
        
    }

    public override float ThrustAmount => Input.GetAxis("Vertical");
}
