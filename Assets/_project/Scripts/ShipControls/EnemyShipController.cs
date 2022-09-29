using System;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class EnemyShipController : ShipController
{
    [SerializeField] float _patrolRange = 2000f, _attackRange = 1000f;
    [SerializeField] LayerMask _targetMask, _playerMask;

    enum EnemyShipState
    {
        None,
        Patrol,
        Attack,
        Reposition,
        Retreat
    };

    AIShipMovementControls _aiShipMovementControls;
    AIShipWeaponControls _aiShipWeaponControls;
    EnemyShipState _state = EnemyShipState.None;
    Transform _transform;

    GameObject PlayerShip => GameObject.FindGameObjectWithTag("Player");
    Transform _target;

    public UnityEvent<int> ShipDestroyed = new UnityEvent<int>();
    
    #region Public data for debugging

    public string ShipState => _state.ToString();
    public string TargetName => _target ? _target.name : "none";

    public string DistanceToTarget
    {
        get
        {
            string distance = String.Empty;
            if (_target)
            {
                distance = $"{Vector3.Distance(_target.position, _transform.position):F2}";
            }
            return distance;
        }
    }

    public string HealthLevel => $"{_damageHandler.Health}/{_damageHandler.MaxHealth}";
    
    #endregion

    bool InAttackRange => Vector3.Distance(PlayerShip.transform.position, _transform.position) <= _attackRange;
    bool ShouldRetreat => _damageHandler.Health < (_damageHandler.MaxHealth * 0.33f);
    bool ReachedPatrolTarget => Vector3.Distance(_target.position, _transform.position) < 0.15f;

    bool ShouldReposition => Physics.SphereCast(_transform.position, 3f, _transform.forward,
        out var hit, 100f, _playerMask);

    public float VectorDifference => (PlayerShip.transform.forward - _transform.forward).magnitude;

    public override void OnEnable()
    {
        _transform = transform;
        _aiShipMovementControls = (AIShipMovementControls)_movementControls;
        _aiShipWeaponControls = (AIShipWeaponControls)_weaponControls;
        SetState(EnemyShipState.Patrol);
        base.OnEnable();
    }

    void OnDisable()
    {
        ShipDestroyed.Invoke(GetInstanceID());
    }

    public override void Update()
    {
        EnemyShipState state = GetNextState();
        SetState(state);
        base.Update();
    }

    EnemyShipState GetNextState()
    {
        EnemyShipState newState = _state switch
        {
            EnemyShipState.Patrol => Patrol(),
            EnemyShipState.Attack => Attack(),
            EnemyShipState.Reposition => Reposition(),
            EnemyShipState.Retreat => Retreat(),
            _ => EnemyShipState.None  
        };
        return newState;
    }

    EnemyShipState Patrol()
    {
        if (ShouldRetreat) return EnemyShipState.Retreat;
        if (InAttackRange) return EnemyShipState.Attack;
        if (ReachedPatrolTarget)
        {
            _target.position = Random.insideUnitSphere * _patrolRange;
        }
            
        return EnemyShipState.Patrol;
    }

    EnemyShipState Attack()
    {
        if (ShouldRetreat) return EnemyShipState.Retreat;
        return ShouldReposition ? EnemyShipState.Reposition : EnemyShipState.Attack;
    }

    EnemyShipState Reposition()
    {
        if (ShouldRetreat) return EnemyShipState.Retreat;
        return Vector3.Distance(_target.position, _transform.position) < 100f
            ? EnemyShipState.Attack
            : EnemyShipState.Reposition;
    }

    EnemyShipState Retreat()
    {
        return EnemyShipState.Retreat;
    }

    void SetState(EnemyShipState state)
    {
        if (_state == state) return;
        _state = state;
        switch (state)
        {
            case EnemyShipState.Patrol:
                if (!_target)
                {
                    _target = new GameObject("Patrol Target").transform;
                    _target.position = Random.insideUnitSphere * _patrolRange;
                    _aiShipMovementControls.SetTarget(_target);
                }
                break;
            case EnemyShipState.Attack:
                if (_target)
                {
                    Destroy(_target.gameObject);
                }

                _target = PlayerShip.transform;
                _aiShipMovementControls.SetTarget(_target);
                SetWeaponsTarget(_target, _attackRange, _targetMask);
                break;
            case EnemyShipState.Reposition:
                _aiShipMovementControls.SetTarget(_target = GetRepositionTarget());
                SetWeaponsTarget(null, 0, 0);
                break;
            case EnemyShipState.Retreat:
                _aiShipMovementControls.SetTarget(_target = GetRetreatTarget());
                SetWeaponsTarget(null, 0, 0);
                break;
        }
    }

    Transform GetRetreatTarget()
    {
        var direction = PlayerShip.transform.position - _transform.position;
        var target = new GameObject("Retreat target").transform;
        target.position = direction * -5000f;
        return target;
    }

    Transform GetRepositionTarget()
    {
        var target = new GameObject("Reposition Target").transform;
        var rand = Random.Range(1, 4);
        var right = _transform.right;
        var up = _transform.up;
        var direction = rand switch
        {
            1 => right,
            2 => right * -1,
            3 => up,
            4 => up * -1,
            _ => _transform.forward * -1
        };
        target.position = _transform.position + direction * 250f;
        return target;
    }

    void SetWeaponsTarget(Transform target, float attackRange, int targetMask)
    {
        foreach (MissileLauncher launcher in _missileLaunchers)
        {
            launcher.SetTarget(target);
        }

        _aiShipWeaponControls.SetTarget(_target, attackRange, targetMask);
    }
}
