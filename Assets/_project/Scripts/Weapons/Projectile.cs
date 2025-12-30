using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Projectile : MonoBehaviour
{
    [SerializeField] Detonator _hitEffect;
    [SerializeField] AudioClip _impactSound;
    
    float _launchForce;
    int _damage;
    float _range;
    float _duration;
    Rigidbody _rigidBody;
    AudioSource _audioSource;
    bool _hasCollided;
    
    bool OutOfFuel
    {
        get
        {
            _duration -= Time.deltaTime;
            return _duration <= 0f;
        }
    }
    

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _audioSource = SoundManager.Configure3DAudioSource(GetComponent<AudioSource>());
    }

    void OnEnable()
    {
        _hasCollided = false;
        _rigidBody.AddForce(_launchForce * transform.forward);
        _duration = _range;
    }

    private void OnDisable()
    {
        _rigidBody.linearVelocity = Vector3.zero;
        _rigidBody.angularVelocity = Vector3.zero;
    }

    void Update()
    {
        if (OutOfFuel) Destroy(gameObject);
    }

    public void Init(int launchForce, int damage, float range, Vector3 velocity, Vector3 angularVelocity)
    {
        _launchForce = launchForce;
        _damage = damage;
        _range = range;
        _rigidBody.linearVelocity = velocity;
        _rigidBody.angularVelocity = angularVelocity;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (_hasCollided) return;
        _hasCollided = true;
        
        if (_impactSound) _audioSource.PlayOneShot(_impactSound);
        if (collision.collider.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            var hitPosition = collision.GetContact(0).point;
            damageable.TakeDamage(_damage, hitPosition);
        }

        if (_hitEffect)
        {
            var hitPosition = collision.GetContact(0).point;
            var hitEffect = Instantiate(_hitEffect, Vector3.zero, Quaternion.identity);
            hitEffect.transform.position = hitPosition;
        }
        Destroy(gameObject);
    }
}
