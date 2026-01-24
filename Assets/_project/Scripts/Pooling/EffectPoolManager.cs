using System.Collections.Generic;
using MidniteOilSoftware.Core;
using UnityEngine;

public class EffectPoolManager : SingletonMonoBehaviour<EffectPoolManager>
{
    [SerializeField] GameObject _detonatorPrefab;
    [SerializeField] GameObject _shieldExplosionPrefab;
    [SerializeField] int _detonatorInitialCapacity = 5;
    [SerializeField] int _detonatorMaxCapacity = 15;
    [SerializeField] int _shieldExplosionInitialCapacity = 3;
    [SerializeField] int _shieldExplosionMaxCapacity = 10;
    
    readonly Dictionary<GameObject, object> _effectStrategies = new();
    
    IPoolStrategy<Detonator> _detonatorStrategy;
    IPoolStrategy<ShieldExplosion> _shieldExplosionStrategy;

    protected override void Awake()
    {
        base.Awake();
        InitializeDefaultStrategies();
    }

    void InitializeDefaultStrategies()
    {
        if (!PoolManager.Instance)
        {
            Debug.LogWarning("PoolManager not found. EffectPoolManager will use fallback instantiation.");
            return;
        }

        var poolRoot = PoolManager.Instance.GetPoolRoot();
        
        if (_detonatorPrefab)
        {
            _detonatorStrategy = new EffectPoolStrategy<Detonator>(
                _detonatorPrefab,
                poolRoot,
                _detonatorInitialCapacity,
                _detonatorMaxCapacity);
            
            _effectStrategies[_detonatorPrefab] = _detonatorStrategy;
        }
        
        if (_shieldExplosionPrefab)
        {
            _shieldExplosionStrategy = new EffectPoolStrategy<ShieldExplosion>(
                _shieldExplosionPrefab,
                poolRoot,
                _shieldExplosionInitialCapacity,
                _shieldExplosionMaxCapacity);
            
            _effectStrategies[_shieldExplosionPrefab] = _shieldExplosionStrategy;
        }
    }

    public T GetStrategy<T>(GameObject prefab) where T : MonoBehaviour, IEffect
    {
        if (_effectStrategies.TryGetValue(prefab, out var strategy))
        {
            return strategy as T;
        }
        return null;
    }

    public IPoolStrategy<Detonator> GetDetonatorStrategy(GameObject prefab = null)
    {
        if (prefab && _effectStrategies.TryGetValue(prefab, out var strategy))
        {
            return strategy as IPoolStrategy<Detonator>;
        }
        return _detonatorStrategy;
    }

    public IPoolStrategy<ShieldExplosion> GetShieldExplosionStrategy(GameObject prefab = null)
    {
        if (prefab && _effectStrategies.TryGetValue(prefab, out var strategy))
        {
            return strategy as IPoolStrategy<ShieldExplosion>;
        }
        return _shieldExplosionStrategy;
    }

    public IPoolStrategy<T> CreateStrategy<T>(GameObject prefab, int initialCapacity = 5, int maxCapacity = 15) where T : MonoBehaviour, IEffect
    {
        if (!PoolManager.Instance) return null;
        
        if (_effectStrategies.TryGetValue(prefab, out var existingStrategy))
        {
            return existingStrategy as IPoolStrategy<T>;
        }

        var poolRoot = PoolManager.Instance.GetPoolRoot();
        var strategy = new EffectPoolStrategy<T>(prefab, poolRoot, initialCapacity, maxCapacity);
        _effectStrategies[prefab] = strategy;
        
        return strategy;
    }

    public Detonator PlayDetonator(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var strategy = GetDetonatorStrategy(prefab);
        if (strategy != null && PoolManager.Instance)
        {
            var detonator = PoolManager.Instance.Get(strategy);
            if (detonator)
            {
                detonator.Play(position, rotation);
                return detonator;
            }
        }
        
        var instance = Instantiate(prefab, position, rotation).GetComponent<Detonator>();
        if (instance)
        {
            instance.Play(position, rotation);
        }
        return instance;
    }

    public ShieldExplosion PlayShieldExplosion(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var strategy = GetShieldExplosionStrategy(prefab);
        if (strategy != null && PoolManager.Instance)
        {
            var explosion = PoolManager.Instance.Get(strategy);
            if (explosion)
            {
                explosion.Play(position, rotation);
                return explosion;
            }
        }
        
        var instance = Instantiate(prefab, position, rotation).GetComponent<ShieldExplosion>();
        if (instance)
        {
            instance.Play(position, rotation);
        }
        return instance;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _effectStrategies.Clear();
    }
}
