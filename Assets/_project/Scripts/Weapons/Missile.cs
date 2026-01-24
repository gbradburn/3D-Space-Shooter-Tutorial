using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Missile : MonoBehaviour, IPoolable
{
    [SerializeField] float _speed = 150f, _rotateSpeed = 150f, _range = 10f, _armingTime = 0.5f;
    [SerializeField] int _damage = 1000;
    [SerializeField] GameObject _explosionPrefab;
    [SerializeField] AudioClip _impactSound;
    
    Transform _transform, _target;
    Rigidbody _rigidbody;
    float _duration, _armDelay;
    Collider _collider;
    AudioSource _audioSource;
    bool _hasCollided;

    IPoolStrategy<Missile> _poolStrategy;

    bool OutOfFuel
    {
        get
        {
            _duration -= Time.deltaTime;
            return _duration <= 0f;
        }
    }

    bool Armed
    {
        get
        {
            _armDelay -= Time.deltaTime;
            return _armDelay <= 0f;
        }
    }

    void Awake()
    {
        _transform = transform;
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        _audioSource = SoundManager.Configure3DAudioSource(GetComponent<AudioSource>());
    }

    public void Init(Transform target)
    {
        _target = target;
    }
    
    public void SetPoolStrategy(IPoolStrategy<Missile> poolStrategy)
    {
        _poolStrategy = poolStrategy;
    }

    void OnEnable()
    {
        _hasCollided = false;
        _duration = _range;
        _armDelay = _armingTime;
        _collider.enabled = false;
    }

    void OnDisable()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    void Update()
    {
        if (OutOfFuel)
        {
            DestroyMissile();
        }

        if (!_collider.enabled && Armed)
        {
            _collider.enabled = true;
        }
    }

    void FixedUpdate()
    {
        if (_target)
        {
            var direction = _target.position - _transform.position;
            var rotation = Quaternion.LookRotation(direction);
            _rigidbody.MoveRotation(Quaternion.RotateTowards(_transform.rotation, rotation, _rotateSpeed * Time.fixedDeltaTime));
        }

        _rigidbody.linearVelocity = _transform.forward * _speed;
    }

    void OnCollisionEnter(Collision other)
    {
        if (_hasCollided) return;
        _hasCollided = true;
        
        if (_impactSound) _audioSource.PlayOneShot(_impactSound);
        if (other.collider.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(_damage, other.GetContact(0).point);
        }
        DestroyMissile();
    }

    void DestroyMissile()
    {
        if (_explosionPrefab)
        {
            EffectManager.PlayEffect(_explosionPrefab, _transform.position, Quaternion.identity);
        }
        ReturnToPool();
    }

    void ReturnToPool()
    {
        if (_poolStrategy != null && PoolManager.Instance)
        {
            PoolManager.Instance.Release(this, _poolStrategy);
        }
        else
        {
            Debug.LogWarning("No pool strategy assigned to missile. Using Destroy fallback.");
            Destroy(gameObject);
        }
    }

    #region IPoolable implementation
    public void OnSpawnedFromPool()
    {
        _hasCollided = false;
    }

    public void OnReturnedToPool()
    {
        _target = null;
        gameObject.SetActive(false);
    }
    
    #endregion IPoolable implementation
}
