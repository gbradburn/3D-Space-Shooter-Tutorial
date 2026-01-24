using UnityEngine;

public class ShieldExplosion : MonoBehaviour, IEffect, IPoolable
{
    [SerializeField] float _duration = 2f;
    [SerializeField] Light _pointLight;
    [SerializeField] ParticleSystem _particleSystem;
    
    float _lightRangeDecay = 2f;
    float _lightIntensityDecay = 1f;
    float _elapsed;
    IPoolStrategy<ShieldExplosion> _poolStrategy;
    bool _isPooled;
    
    void Update()
    {
        _elapsed += Time.deltaTime;
        
        if (_pointLight)
        {
            if (_pointLight.range > 0)
            {
                _pointLight.range -= _lightRangeDecay * Time.deltaTime;
            }

            if (_pointLight.intensity > 0)
            {
                _pointLight.intensity -= _lightIntensityDecay * Time.deltaTime;
            }
        }
        
        if (_elapsed >= _duration)
        {
            if (_isPooled && _poolStrategy != null && PoolManager.Instance)
            {
                PoolManager.Instance.Release(this, _poolStrategy);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
    public void SetPoolStrategy(IPoolStrategy<ShieldExplosion> poolStrategy)
    {
        _poolStrategy = poolStrategy;
        _isPooled = true;
    }

    #region IEffect implementation
    public float Duration => _duration;
    
    public void Play(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        _elapsed = 0f;
        
        if (_particleSystem)
        {
            _particleSystem.Play();
        }
    }
    #endregion IEffect implementation

    #region IPoolable implementation
    public void OnSpawnedFromPool()
    {
        _elapsed = 0f;
    }

    public void OnReturnedToPool()
    {
        if (_particleSystem)
        {
            _particleSystem.Stop();
            _particleSystem.Clear();
        }
        gameObject.SetActive(false);
    }
    #endregion IPoolable implementation
}
