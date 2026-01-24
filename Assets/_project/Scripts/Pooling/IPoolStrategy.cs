using UnityEngine;

public interface IPoolStrategy<T> where T : Component
{
    T OnCreate();
    void OnGet(T projectile);
    void OnRelease(T projectile);
    void OnDestroy(T projectile);
    
    GameObject Prefab { get; }
    int InitialCapacity { get; }
    int MaxCapacity { get; }
}
