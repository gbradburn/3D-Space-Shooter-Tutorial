using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Blaster : MonoBehaviour
{
    [SerializeField] [Required] Projectile _projectilePrefab;

    [SerializeField] Transform _muzzle;
    [SerializeField] [Range(0f, 5f)] float _coolDownTime = 0.25f;

    private IWeaponControls _weaponInput;
    
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

    public void Init(IWeaponControls weaponInput)
    {
        _weaponInput = weaponInput;
    }
    
    void FireProjectile()
    {
        _coolDown = _coolDownTime;
        Instantiate(_projectilePrefab, _muzzle.position, transform.rotation);
    }

}
