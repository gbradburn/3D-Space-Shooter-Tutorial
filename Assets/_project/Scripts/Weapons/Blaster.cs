using Sirenix.OdinInspector;
using UnityEngine;

public class Blaster : MonoBehaviour
{
    [SerializeField] [Required] Projectile _projectilePrefab;

    [SerializeField] Transform _muzzle;
    
    float _coolDownTime;
    int _launchForce, _damage;
    private float _duration;
    IWeaponControls _weaponInput;
    
    bool CanFire
    {
        get
        {
            _coolDown -= Time.deltaTime;
            return _coolDown <= 0f;
        }
    }
    
    float _coolDown;
    
    // Update is called once per frame
    void Update()
    {
        if (_weaponInput == null) return;
        if (CanFire && _weaponInput.PrimaryFired)
        {
            FireProjectile();
        } 
    }

    public void Init(IWeaponControls weaponInput, float coolDown, int launchForce, float duration, int damage)
    {
        Debug.Log($"Blaster.Init({weaponInput}, {coolDown}, launchForce, {duration}");
        _weaponInput = weaponInput;
        _coolDownTime = coolDown;
        _launchForce = launchForce;
        _duration = duration;
        _damage = damage;
    }
    
    void FireProjectile()
    {
        _coolDown = _coolDownTime;
        Projectile projectile = Instantiate(_projectilePrefab, _muzzle.position, transform.rotation);
        projectile.gameObject.SetActive(false);
        projectile.Init(_launchForce, _damage, _duration);
        projectile.gameObject.SetActive(true);
    }

}
