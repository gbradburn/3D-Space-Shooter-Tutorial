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
        Debug.Log($"{name} velocity={_rigidBody.linearVelocity.magnitude}");
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
        if (_impactSound) _audioSource.PlayOneShot(_impactSound);
        IDamageable damageable = collision.collider.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            Vector3 hitPosition = collision.GetContact(0).point;
            damageable.TakeDamage(_damage, hitPosition);
        }

        if (_hitEffect != null)
        {
            Instantiate(_hitEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
