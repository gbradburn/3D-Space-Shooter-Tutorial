using UnityEngine;

public class EffectPoolStrategy<T> : IPoolStrategy<T> where T : MonoBehaviour, IEffect
{
    readonly GameObject _prefab;
    readonly Transform _poolRoot;
    readonly int _initialCapacity;
    readonly int _maxSize;

    public GameObject Prefab => _prefab;
    public int InitialCapacity => _initialCapacity;
    public int MaxCapacity => _maxSize;
    
    public EffectPoolStrategy(
        GameObject prefab, 
        Transform poolRoot,
        int initialCapacity = 5,
        int maxSize = 15)
    {
        _prefab = prefab;
        _poolRoot = poolRoot;
        _initialCapacity = initialCapacity;
        _maxSize = maxSize;
    }

    public T OnCreate()
    {
        var instance = Object.Instantiate(_prefab, _poolRoot).GetComponent<T>();
        
        if (instance is Detonator detonator)
        {
            detonator.SetPoolStrategy(this as IPoolStrategy<Detonator>);
        }
        else if (instance is ShieldExplosion shieldExplosion)
        {
            shieldExplosion.SetPoolStrategy(this as IPoolStrategy<ShieldExplosion>);
        }
        
        if (instance is IPoolable)
        {
            instance.gameObject.SetActive(false);
        }
        
        return instance;
    }

    public void OnGet(T effect)
    {
        effect.gameObject.SetActive(true);
            
        if (effect is IPoolable poolable)
            poolable.OnSpawnedFromPool();
    }

    public void OnRelease(T effect)
    {
        if (effect is IPoolable poolable)
            poolable.OnReturnedToPool();
            
        effect.gameObject.SetActive(false);
        effect.transform.SetParent(_poolRoot);
    }

    public void OnDestroy(T effect)
    {
        if (effect && effect.gameObject)
            Object.Destroy(effect.gameObject);
    }
}
