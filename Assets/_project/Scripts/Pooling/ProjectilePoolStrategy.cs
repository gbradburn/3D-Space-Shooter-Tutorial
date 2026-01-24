using UnityEngine;

public class ProjectilePoolStrategy : IPoolStrategy<Projectile>
{
    readonly GameObject _prefab;
    readonly Transform _poolRoot;
    readonly int _initialCapacity;
    readonly int _maxSize;

    public GameObject Prefab => _prefab;
    public int InitialCapacity => _initialCapacity;
    public int MaxCapacity => _maxSize;    
    
    public ProjectilePoolStrategy(
        GameObject prefab, 
        Transform poolRoot,
        int initialCapacity = 50,
        int maxSize = 100)
    {
        _prefab = prefab;
        _poolRoot = poolRoot;
        _initialCapacity = initialCapacity;
        _maxSize = maxSize;
    }

    public Projectile OnCreate()
    {
        var instance = Object.Instantiate(_prefab, _poolRoot).GetComponent<Projectile>();
        instance.SetPoolStrategy(this);
        instance.gameObject.SetActive(false);
        return instance;
    }

    public void OnGet(Projectile projectile)
    {
        projectile.gameObject.SetActive(true);
            
        if (projectile is IPoolable poolable)
            poolable.OnSpawnedFromPool();
    }

    public void OnRelease(Projectile projectile)
    {
        if (projectile is IPoolable poolable)
            poolable.OnReturnedToPool();
            
        projectile.gameObject.SetActive(false);
        projectile.transform.SetParent(_poolRoot);
    }

    public void OnDestroy(Projectile projectile)
    {
        if (projectile && projectile.gameObject)
            Object.Destroy(projectile.gameObject);
    }
}