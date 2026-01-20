using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.SpaceShooter.Events;

public class ShipController : MonoBehaviour
{
    [SerializeField] Shield _shield;
    [SerializeField]
    protected MovementControlsBase _movementControls;

    [SerializeField]
    protected WeaponControlsBase _weaponControls;    

    [SerializeField]
    ShipDataSo _shipData;
    
    [SerializeField]
    List<ShipEngine> _engines;

    [SerializeField]
    List<Blaster> _blasters;

    [SerializeField] protected List<MissileLauncher> _missileLaunchers;
    
    [SerializeField]
    private AnimateCockpitControls _cockpitAnimationControls;
    
    Rigidbody _rigidBody;
    [Range(-1f, 1f)]
    float _pitchAmount, _rollAmount, _yawAmount = 0f;

    protected DamageHandler _damageHandler;
    bool _isLocalPlayer;

    IMovementControls MovementInput => _movementControls;
    IWeaponControls WeaponInput => _weaponControls;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _damageHandler = GetComponent<DamageHandler>();

        if (!_movementControls)
        {
            _movementControls = GetComponent<MovementControlsBase>();
        }

        if (!_weaponControls)
        {
            _weaponControls = GetComponent<WeaponControlsBase>();
        }
    }
    void Start()
    {
        InitializeWeaponSystems();
        RaiseWeaponSystemsInitializedEvent();
    }

    void InitializeWeaponSystems()
    {
        foreach (var engine in _engines)
        {
            if (engine) engine.Init(MovementInput, _rigidBody, _shipData.ThrustForce / _engines.Count);
        }

        foreach (var blaster in _blasters)
        {
            if (blaster) blaster.Init(WeaponInput, _shipData.BlasterCooldown, _shipData.BlasterLaunchForce, _shipData.BlasterProjectileDuration, _shipData.BlasterDamage, _rigidBody);
        }

        foreach (var launcher in _missileLaunchers)
        {
            if (launcher) launcher.Init(WeaponInput);
        }

        if (_cockpitAnimationControls)
        {
            _cockpitAnimationControls.Init(MovementInput);
        }

        if (_shield)
        {
            _shield.Init(_shipData.ShieldStrength);
        }
    }

    void RaiseWeaponSystemsInitializedEvent()
    {
        EventBus.Instance.Raise(new WeaponSystemsInitializedEvent(
            _blasters.ToArray(),
            _missileLaunchers.ToArray(),
            _isLocalPlayer
        ));
    }

    public virtual void OnEnable()
    {
        if (_damageHandler == null) return;
        _damageHandler.Init(_shipData.MaxHealth);
        _damageHandler.HealthChanged.AddListener(OnHealthChanged);
        _damageHandler.ObjectDestroyed.AddListener(DestroyShip);
    }

    public virtual void Update()
    {
        if (MovementInput == null) return;

        _rollAmount = MovementInput.RollAmount;
        _yawAmount = MovementInput.YawAmount;
        _pitchAmount = MovementInput.PitchAmount;
        
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = !UnityEditor.EditorApplication.isPaused;
#endif
        }
    }

    public void SetIsLocalPlayer(bool isLocalPlayer)
    {
        _isLocalPlayer = isLocalPlayer;
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
    
    void DestroyShip()
    {
        gameObject.SetActive(false);
    }

    void OnHealthChanged()
    {
    }


}
