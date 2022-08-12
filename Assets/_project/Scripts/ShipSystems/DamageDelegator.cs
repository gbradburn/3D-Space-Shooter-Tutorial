using UnityEngine;

public class DamageDelegator : MonoBehaviour, IDamageable
{
    [SerializeField] GameObject _target;

    IDamageable _targetDamageable;

    void Awake()
    {
        if (_target)
        {
            _target.TryGetComponent(out _targetDamageable);
        }
    }

    public void TakeDamage(int damage, Vector3 hitPosition)
    {
        Debug.Log($"{gameObject.name} was hit.");
        if (_targetDamageable != null)
        {
            _targetDamageable.TakeDamage(damage, hitPosition);
        }
    }
}
