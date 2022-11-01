using System;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Turret : MonoBehaviour
{
    [SerializeField] Transform _base, _guns, _target;
    [SerializeField] float _rotateSpeed = 100f;

    Vector3 TargetPosition => _target ? _target.position : Vector3.zero;
    Quaternion _baseTargetDirection;
    Vector3 _gunsTargetElevation;

    void Update()
    {
        if (!_target) return;
        HorizontalRotation();
        VerticalRotation();
    }

    void LateUpdate()
    {
        if (!_target) return;
        var from = Quaternion.LookRotation(_base.forward, _base.up);
        _base.rotation = Quaternion.RotateTowards(from, _baseTargetDirection, _rotateSpeed * Time.deltaTime);
        _guns.localEulerAngles = _gunsTargetElevation;
    }

    void HorizontalRotation()
    {
        // Rotate turret base on y-axis
        var up = _base.up;
        var directionToTarget = Vector3.ProjectOnPlane(TargetPosition - _base.position, up);
        _baseTargetDirection = Quaternion.LookRotation(directionToTarget, up);
    }

    void VerticalRotation()
    {
        var targetPosition = _guns.InverseTransformDirection(_target.position - _guns.position);
        var projectedAngle = Vector3.ProjectOnPlane(targetPosition, Vector3.up);
        var elevation = Vector3.Angle(projectedAngle, targetPosition) * Math.Sign(targetPosition.y);
        if (Mathf.Abs(elevation) > Mathf.Epsilon)
        {
            // Rotate guns on the x-axis
            _gunsTargetElevation = Vector3.right * -elevation;
        }
        else
        {
            _gunsTargetElevation = Vector3.zero;
        }
    }
}
