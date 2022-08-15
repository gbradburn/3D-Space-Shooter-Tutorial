using UnityEngine;

[CreateAssetMenu(fileName = "ShipData", menuName = "3D Space Shooter/Ship Data", order = 1)]
public class ShipDataSo : ScriptableObject
{
    [SerializeField] [Range(1000f, 10000f)]
    float _thrustForce = 7500f,
        _pitchForce = 6000f,
        _rollForce = 1000f,
        _yawForce = 2000f;

    [SerializeField]
    private int _shieldStrength = 5000,
        _shieldRegenAmount = 100,
        _maxHealth = 5000;
        
    [SerializeField] [Range(10, 1000)]         
    int _blasterDamage = 100;
    
    [Range(5000f, 25000f)]
    [SerializeField]
    int _blasterLaunchForce = 10000;
    [SerializeField]
    float _blasterCoolDown = 0.25f;
    [SerializeField] [Range(2f, 10f)]     
    float _blasterProjectileDuration = 2f;


    public float ThrustForce => _thrustForce;
    public float PitchForce => _pitchForce;
    public float RollForce => _rollForce;
    public float YawForce => _yawForce;
    public int ShieldStrength => _shieldStrength;
    public int ShieldRegenAmount => _shieldRegenAmount;
    public int MaxHealth => _maxHealth;
    public int BlasterLaunchForce => _blasterLaunchForce;
    public int BlasterDamage => _blasterDamage;
    public float BlasterProjectileDuration => _blasterProjectileDuration;
    public float BlasterCooldown => _blasterCoolDown;
}
