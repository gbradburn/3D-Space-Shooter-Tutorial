using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Blaster : MonoBehaviour
{
    [SerializeField] Projectile _projectilePrefab, _weakProjectilePrefab;
    [SerializeField] AudioClip _fireSound;
    [SerializeField] Transform _muzzle;

    public float CapacitorChargePercentage => _capacitor / _maxCapacitor;
    public float CoolDownPercent => Mathf.Clamp(_coolDown / _coolDownTime, 0f, 1f);
    
    float _capacitor, _maxCapacitor, _costPerShot, _rechargeRate;
    float _coolDownTime;
    int _launchForce, _damage;
    float _duration;
    IWeaponControls _weaponInput;
    float _coolDown;
    Rigidbody _rigidBody;

    AudioSource _audioSource;
    
    bool CanFire
    {
        get
        {
            _coolDown -= Time.deltaTime;
            return _coolDown <= 0f;
        }
    }

    void Awake()
    {
        _audioSource = SoundManager.Configure3DAudioSource(GetComponent<AudioSource>());
    }


    void Update()
    {
        _capacitor = Mathf.Min(_maxCapacitor, _capacitor + (_rechargeRate * Time.deltaTime));
        if (_weaponInput == null) return;
        if (CanFire && _weaponInput.PrimaryFired)
        {
            FireProjectile();
        } 
    }

    public void Init(
        IWeaponControls weaponInput, 
        float coolDown, 
        int launchForce, 
        float duration, 
        int damage, 
        Rigidbody rigidBody,
        float maxCapacitor=1000f,
        float costPerShot=50f,
        float rechargeRate=20f)
    {
        _weaponInput = weaponInput;
        _coolDownTime = coolDown;
        _launchForce = launchForce;
        _duration = duration;
        _damage = damage;
        _rigidBody = rigidBody;
        _capacitor = _maxCapacitor = maxCapacitor;
        _costPerShot = costPerShot;
        _rechargeRate = rechargeRate;
    }
    
    void FireProjectile()
    {
        if (_fireSound)
        {
            _audioSource.PlayOneShot(_fireSound);
        }
        _coolDown = _coolDownTime;
        bool fullCharge = _capacitor >= _costPerShot;
        _capacitor = Mathf.Max(_capacitor - _costPerShot, 0);
        Projectile projectile = Instantiate(fullCharge ? _projectilePrefab : _weakProjectilePrefab, _muzzle.position, transform.rotation);
        projectile.gameObject.SetActive(false);
        projectile.Init(_launchForce, fullCharge ? _damage : (int)(_damage * 0.1f), _duration, _rigidBody.linearVelocity, _rigidBody.angularVelocity);
        projectile.gameObject.SetActive(true);
    }

}
