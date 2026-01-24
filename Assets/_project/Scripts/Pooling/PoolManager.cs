using System;
using System.Collections.Generic;
using MidniteOilSoftware.Core;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : SingletonMonoBehaviour<PoolManager>
{
    public Transform GetPoolRoot() => _poolRoot;
    
    readonly Dictionary<Type, object> _pools = new();
    Transform _poolRoot;

    protected override void Awake()
    {
        base.Awake();
        _poolRoot = new GameObject("PooledObjects").transform;
        _poolRoot.SetParent(transform);
    }
    
    public ObjectPool<T> GetOrCreatePool<T>(IPoolStrategy<T> strategy) where T : Component
    {
        var type = typeof(T);

        if (_pools.TryGetValue(type, out var existingPool))
        {
            return (ObjectPool<T>)existingPool;
        }

        var pool = new ObjectPool<T>(
            createFunc: strategy.OnCreate,
            actionOnGet: strategy.OnGet,
            actionOnRelease: strategy.OnRelease,
            actionOnDestroy: strategy.OnDestroy,
            collectionCheck: true,
            defaultCapacity: strategy.InitialCapacity,
            maxSize: strategy.MaxCapacity
        );

        _pools[type] = pool;
        return pool;    
    }
    
    public T Get<T>(IPoolStrategy<T> strategy) where T : Component
    {
        var pool = GetOrCreatePool(strategy);
        if (_enableDebugLog)
        {
            Debug.Log($"Getting instance of {typeof(T).Name} from pool.", this);
        }
        return pool.Get();
    }
    
    public void Release<T>(T item, IPoolStrategy<T> strategy) where T : Component
    {
        var pool = GetOrCreatePool(strategy);
        if (_enableDebugLog)
        {
            Debug.Log($"Releasing instance of {typeof(T).Name} back to pool.", this);
        }
        pool.Release(item);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _pools.Clear();
    }
}