using UnityEngine;

public class MissilePoolStrategy : IPoolStrategy<Missile>
{
    readonly GameObject _prefab;
    readonly Transform _poolRoot;
    readonly int _initialCapacity;
    readonly int _maxSize;

    public GameObject Prefab => _prefab;
    public int InitialCapacity => _initialCapacity;
    public int MaxCapacity => _maxSize;
    
    public MissilePoolStrategy(
        GameObject prefab, 
        Transform poolRoot,
        int initialCapacity = 10,
        int maxSize = 20)
    {
        _prefab = prefab;
        _poolRoot = poolRoot;
        _initialCapacity = initialCapacity;
        _maxSize = maxSize;
    }

    public Missile OnCreate()
    {
        var instance = Object.Instantiate(_prefab, _poolRoot).GetComponent<Missile>();
        instance.SetPoolStrategy(this);
        instance.gameObject.SetActive(false);
        return instance;
    }

    public void OnGet(Missile missile)
    {
        missile.gameObject.SetActive(true);
            
        if (missile is IPoolable poolable)
            poolable.OnSpawnedFromPool();
    }

    public void OnRelease(Missile missile)
    {
        if (missile is IPoolable poolable)
            poolable.OnReturnedToPool();
            
        missile.gameObject.SetActive(false);
        missile.transform.SetParent(_poolRoot);
    }

    public void OnDestroy(Missile missile)
    {
        if (missile && missile.gameObject)
            Object.Destroy(missile.gameObject);
    }
}
