using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField]
    MovementControlsBase _movementControls;

    [SerializeField]
    WeaponControlsBase _weaponControls;    

    [SerializeField]
    ShipDataSo _shipData;
    
    [SerializeField]
    List<ShipEngine> _engines;

    [SerializeField]
    List<Blaster> _blasters;

    [SerializeField]
    private AnimateCockpitControls _cockpitAnimationControls;
    
    Rigidbody _rigidBody;
    [Range(-1f, 1f)]
    float _pitchAmount, _rollAmount, _yawAmount = 0f;

    IMovementControls MovementInput => _movementControls;
    IWeaponControls WeaponInput => _weaponControls;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }
    void Start()
    {
        foreach (ShipEngine engine in _engines)
        {
            engine.Init(MovementInput, _rigidBody, _shipData.ThrustForce / _engines.Count);
        }

        foreach (Blaster blaster in _blasters)
        {
            blaster.Init(WeaponInput, _shipData.BlasterCooldown, _shipData.BlasterLaunchForce, _shipData.BlasterProjectileDuration, _shipData.BlasterDamage);
        }

        if (_cockpitAnimationControls != null)
        {
            _cockpitAnimationControls.Init(MovementInput);
        }
    }

    void Update()
    {
        _rollAmount = MovementInput.RollAmount;
        _yawAmount = MovementInput.YawAmount;
        _pitchAmount = MovementInput.PitchAmount;
    }

    void FixedUpdate()
    {
        if (!Mathf.Approximately(0f, _pitchAmount))
        {
            _rigidBody.AddTorque(transform.right * (_shipData.PitchForce * _pitchAmount * Time.fixedDeltaTime));
        }

        if (!Mathf.Approximately(0f, _rollAmount))
        {
            _rigidBody.AddTorque(transform.forward * (_shipData.RollForce * _rollAmount * Time.fixedDeltaTime));
        }

        if (!Mathf.Approximately(0f, _yawAmount))
        {
            _rigidBody.AddTorque(transform.up * (_yawAmount * _shipData.YawForce * Time.fixedDeltaTime));
        }
    }
}
