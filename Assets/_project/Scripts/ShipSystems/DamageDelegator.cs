using System.Collections.Generic;
using UnityEngine;

public class DamageDelegator : MonoBehaviour, IDamageable
{
    [SerializeField] GameObject[] _damageDelegates;

    List<IDamageable> _damageReceivers;

    void Awake()
    {
        if (_damageDelegates == null) return;
        _damageReceivers = new List<IDamageable>();
        foreach (GameObject damageDelegate in _damageDelegates)
        {
            if (damageDelegate.TryGetComponent<IDamageable>(out var damageable))
            {
                _damageReceivers.Add(damageable);
            }
        }
    }

    public void TakeDamage(int damage, Vector3 hitPosition)
    {
        if (_damageReceivers == null) return;
        foreach (IDamageable damageable in _damageReceivers)
        {
            damageable.TakeDamage(damage, hitPosition);
        }
    }
}
