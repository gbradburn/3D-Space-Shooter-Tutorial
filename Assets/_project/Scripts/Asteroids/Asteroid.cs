using UnityEditor;
using UnityEngine;

public class Asteroid : MonoBehaviour, IDamageable
{
    [SerializeField] private FracturedAsteroid _fracturedAsteroidPrefab;
    [SerializeField] private GameObject _explosionPrefab;

    private Transform _transform;
    private bool _isDestroyed;

    private void Awake()
    {
        _transform = transform;
    }

    public void TakeDamage(int damage, Vector3 hitPosition)
    {
        if (_isDestroyed) return;
        _isDestroyed = true;
        
        FractureAsteroid(hitPosition);
    }

    private void FractureAsteroid(Vector3 hitPosition)
    {
        var asteroidPosition = _transform.position;
        
        if (_fracturedAsteroidPrefab)
        {
            Instantiate(_fracturedAsteroidPrefab, asteroidPosition, _transform.rotation);
        }

        if (_explosionPrefab)
        {
            Instantiate(_explosionPrefab, asteroidPosition, Quaternion.identity);
        }
        
        gameObject.name = "Destroyed Asteroid";
        gameObject.SetActive(false);
        Destroy(gameObject, 5f);
    }
}