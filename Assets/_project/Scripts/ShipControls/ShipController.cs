using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class ShipController : MonoBehaviour
{
    [FormerlySerializedAs("_movementInput")]
    [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
    [SerializeField] 
    [Required] 
    ShipInputControls _inputControls;
    
    [BoxGroup("Ship movement values")] [SerializeField] [Range(1000f, 10000f)]
    float _thrustForce = 7500f,
        _pitchForce = 6000f,
        _rollForce = 1000f,
        _yawForce = 2000f;

    [BoxGroup("Ship components")] [SerializeField] [Required]
    List<ShipEngine> _engines;

    [BoxGroup("Ship components")] [SerializeField]
    List<Blaster> _blasters;

    [BoxGroup("Ship components")] [SerializeField]
    private AnimateCockpitControls _cockpitAnimationControls;
    
    Rigidbody _rigidBody;
    [ShowInInspector] [Range(-1f, 1f)]
    float _pitchAmount, _rollAmount, _yawAmount = 0f;

    IMovementControls MovementInput => _inputControls.MovementControls;
    IWeaponControls WeaponInput => _inputControls.WeaponControls;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }
    void Start()
    {
        foreach (ShipEngine engine in _engines)
        {
            engine.Init(MovementInput, _rigidBody, _thrustForce / _engines.Count);
        }

        foreach (Blaster blaster in _blasters)
        {
            blaster.Init(WeaponInput);
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
    }
}
