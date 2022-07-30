using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class ShipController : MonoBehaviour
{
    [BoxGroup("Ship input controls")]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    [SerializeField]
    [Required]
    MovementControlsBase _movementControls;

    [BoxGroup("Ship input controls")] [InlineEditor(InlineEditorObjectFieldModes.Foldout)] [SerializeField] [Required]
    WeaponControlsBase _weaponControls;    

    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    [SerializeField] [Required]
    ShipDataSo _shipData;
    
    [BoxGroup("Ship components")] [SerializeField] [Required]
    List<ShipEngine> _engines;

    [BoxGroup("Ship components")] [SerializeField]
    List<Blaster> _blasters;

    [BoxGroup("Ship components")] [SerializeField]
    private AnimateCockpitControls _cockpitAnimationControls;
    
    Rigidbody _rigidBody;
    [ShowInInspector] [Range(-1f, 1f)]
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
