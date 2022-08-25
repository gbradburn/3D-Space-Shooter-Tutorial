using UnityEngine;

public class Blaster : MonoBehaviour
{
    [SerializeField] Projectile _projectilePrefab;

    [SerializeField] Transform _muzzle;
    
    float _coolDownTime;
    int _launchForce, _damage;
    float _duration;
    IWeaponControls _weaponInput;
    float _coolDown;
    Rigidbody _rigidBody;
    
    bool CanFire
    {
        get
        {
            _coolDown -= Time.deltaTime;
            return _coolDown <= 0f;
        }
    }
    

    void Update()
    {
        if (_weaponInput == null) return;
        if (CanFire && _weaponInput.PrimaryFired)
        {
            FireProjectile();
        } 
    }

    public void Init(IWeaponControls weaponInput, float coolDown, int launchForce, float duration, int damage, Rigidbody rigidBody)
    {
        _weaponInput = weaponInput;
        _coolDownTime = coolDown;
        _launchForce = launchForce;
        _duration = duration;
        _damage = damage;
        _rigidBody = rigidBody;
    }
    
    void FireProjectile()
    {
        _coolDown = _coolDownTime;
        Projectile projectile = Instantiate(_projectilePrefab, _muzzle.position, transform.rotation);
        projectile.gameObject.SetActive(false);
        projectile.Init(_launchForce, _damage, _duration, _rigidBody.velocity, _rigidBody.angularVelocity);
        projectile.gameObject.SetActive(true);
    }

}
