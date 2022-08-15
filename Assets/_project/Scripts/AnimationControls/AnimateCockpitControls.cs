using System.Collections.Generic;
using UnityEngine;

public class AnimateCockpitControls : MonoBehaviour
{
    [SerializeField] 
    Transform _joystick;
    
    [SerializeField] 
    Vector3 _joystickRange = new Vector3(35,35,35);

    [SerializeField]
    List<Transform> _throttles;

    [SerializeField]
    float _throttleRange = 35f;

    private IMovementControls _movementInput;
    
    // Update is called once per frame
    void Update()
    {
        if (_movementInput == null) return;
        _joystick.localRotation = Quaternion.Euler(
            _movementInput.PitchAmount * _joystickRange.x,
            _movementInput.YawAmount * _joystickRange.y,
            _movementInput.RollAmount * _joystickRange.z
        );

        Vector3 throttleRotation = _throttles[0].localRotation.eulerAngles;
        throttleRotation.x = _movementInput.ThrustAmount * _throttleRange;
        foreach (Transform throttle in _throttles)
        {
            throttle.localRotation = Quaternion.Euler(throttleRotation);
        }
    }

    public void Init(IMovementControls movementControls)
    {
        _movementInput = movementControls;
    }
}
