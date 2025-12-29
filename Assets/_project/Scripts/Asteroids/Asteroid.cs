using System;
using UnityEngine;

public class Asteroid : MonoBehaviour, IDamageable
{
    [SerializeField] private FracturedAsteroid _fracturedAsteroidPrefab;
    [SerializeField] private GameObject _explosionPrefab;

    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    public void TakeDamage(int damage, Vector3 hitPosition)
    {
        FractureAsteroid(hitPosition);
    }

    private void FractureAsteroid(Vector3 hitPosition)
    {
        if (_fracturedAsteroidPrefab)
        {
            Instantiate(_fracturedAsteroidPrefab, _transform.position, _transform.rotation);
        }

        if (_explosionPrefab)
        {
            var explosion = Instantiate(_explosionPrefab, transform.position/*hitPosition*/, Quaternion.identity);
            Debug.Log($"explosion instantiated at {explosion.transform.position}, asteroid position {transform.position}");
        }
        Destroy(gameObject);
        Invoke(nameof(PauseGame), 0.5f);
    }

    void PauseGame()
    {
        UnityEditor.EditorApplication.isPaused = true;
    }
}