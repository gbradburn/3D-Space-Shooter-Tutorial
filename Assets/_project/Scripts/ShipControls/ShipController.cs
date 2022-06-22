using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ShipController : MonoBehaviour
{
 //   [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
    [SerializeField] 
    //[Required] 
    ShipMovementInput _movementInput;
    
    [BoxGroup("Ship movement values")] [SerializeField] [Range(1000f, 10000f)]
    float _thrustForce = 7500f,
        _pitchForce = 6000f,
        _rollForce = 1000f,
        _yawForce = 2000f;

    Rigidbody _rigidBody;
    [ShowInInspector] [Range(-1f, 1f)]
    float _thrustAmount, _pitchAmount, _rollAmount, _yawAmount = 0f;

    IMovementControls ControlInput => _movementInput.MovementControls;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        _thrustAmount = ControlInput.ThrustAmount;
        _rollAmount = ControlInput.RollAmount;
        _yawAmount = ControlInput.YawAmount;
        _pitchAmount = ControlInput.PitchAmount;
    }

    void FixedUpdate()
    {
        if (!Mathf.Approximately(0f, _pitchAmount))
        {
            _rigidBody.AddTorque(transform.right * (_pitchForce * _pitchAmount * Time.fixedDeltaTime));
        }

        if (!Mathf.Approximately(0f, _rollAmount))
        {
            _rigidBody.AddTorque(transform.forward * (_rollForce * _rollAmount * Time.fixedDeltaTime));
        }

        if (!Mathf.Approximately(0f, _yawAmount))
        {
            _rigidBody.AddTorque(transform.up * (_yawAmount * _yawForce * Time.fixedDeltaTime));
        }

        if (!Mathf.Approximately(0f, _thrustAmount))
        {
            _rigidBody.AddForce(transform.forward * (_thrustForce * _thrustAmount * Time.fixedDeltaTime));
        }
    }
}
