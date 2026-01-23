using System;
using UnityEngine;
using UnityEngine.Events;

public class DamageHandler : MonoBehaviour, IDamageable
{
    [SerializeField] GameObject _explosionPrefab;
    UnityEvent _healthChangedEvent;
    UnityEvent _objectDestroyedEvent;
    public int MaxHealth { get; private set; }
    public int Health { get; private set; }
    public GameObject LastExplosion { get; private set; }

    public UnityEvent HealthChanged => _healthChangedEvent ??= new UnityEvent();
    public UnityEvent ObjectDestroyed => _objectDestroyedEvent ??= new UnityEvent();
    
    bool _isDestroyed;

    void OnDestroy()
    {
        Debug.Log($"{name} destroyed.", this);
    }

    public void Init(int maxHealth)
    {
        Health = MaxHealth = maxHealth;
        _isDestroyed = false;
        HealthChanged.Invoke();
    }

    public void TakeDamage(int damage, Vector3 hitPosition)
    {
        if (_isDestroyed) return;
        Health -= damage;
        HealthChanged.Invoke();
        if (Health > 0) return;
        
        _isDestroyed = true;
        
        if (_explosionPrefab)
        {
            LastExplosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        }
        
        ObjectDestroyed.Invoke();
        gameObject.SetActive(false);
    }
}
