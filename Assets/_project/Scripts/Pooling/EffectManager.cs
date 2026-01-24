using UnityEngine;

public static class EffectManager
{
    public static T PlayEffect<T>(GameObject prefab, Vector3 position, Quaternion rotation, IPoolStrategy<T> strategy = null) where T : MonoBehaviour, IEffect
    {
        if (!prefab) return null;
        
        T effect;
        
        if (strategy == null && EffectPoolManager.Instance)
        {
            if (typeof(T) == typeof(Detonator))
            {
                strategy = EffectPoolManager.Instance.GetDetonatorStrategy(prefab) as IPoolStrategy<T>;
            }
            else if (typeof(T) == typeof(ShieldExplosion))
            {
                strategy = EffectPoolManager.Instance.GetShieldExplosionStrategy(prefab) as IPoolStrategy<T>;
            }
        }
        
        if (strategy != null && PoolManager.Instance)
        {
            effect = PoolManager.Instance.Get(strategy);
            if (effect)
            {
                effect.Play(position, rotation);
                return effect;
            }
        }
        
        effect = Object.Instantiate(prefab, position, rotation).GetComponent<T>();
        if (effect)
        {
            effect.Play(position, rotation);
        }
        
        return effect;
    }
    
    public static GameObject PlayEffect(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!prefab) return null;
        
        if (EffectPoolManager.Instance)
        {
            var detonator = prefab.GetComponent<Detonator>();
            if (detonator)
            {
                return EffectPoolManager.Instance.PlayDetonator(prefab, position, rotation)?.gameObject;
            }
            
            var shieldExplosion = prefab.GetComponent<ShieldExplosion>();
            if (shieldExplosion)
            {
                return EffectPoolManager.Instance.PlayShieldExplosion(prefab, position, rotation)?.gameObject;
            }
        }
        
        return Object.Instantiate(prefab, position, rotation);
    }
}
