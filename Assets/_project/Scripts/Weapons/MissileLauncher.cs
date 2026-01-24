using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class MissileLauncher : MonoBehaviour
{
    [SerializeField] GameObject _missilePrefab;
    [SerializeField] int _missiles = 4, _reloads = 2;
    [SerializeField] float _coolDown = 2f, _reloadTime = 60f;
    [SerializeField] AudioClip _launchSound;
    [SerializeField] int _initialPoolSize = 10, _maxPoolSize = 20;
    
    public int MissileCapacity => _missiles;
    public int Missiles => _missilesRemaining;
    public float ReloadPercent => 1f - (_reloadDelay / _reloadTime);
    public int Reloads => _reloadsRemaining;
    public bool Reloading => _missilesRemaining < 1 && _reloadsRemaining > 0 && _reloadDelay > 0f;

    public UnityEvent MissileFired, MissilesReloaded;
    
    Transform _transform, _target;
    RadarScreen _radarScreen;
    int _missilesRemaining, _reloadsRemaining;
    float _fireDelay, _reloadDelay;
    AudioSource _audioSource;
    IWeaponControls _weaponInput;
    
    IPoolStrategy<Missile> _missileStrategy;

    bool CanFire
    {
        get
        {
            if (_missilesRemaining < 1) return false;
            _fireDelay -= Time.deltaTime;
            return _fireDelay <= 0f;
        }
    }

    bool CanReload
    {
        get
        {
            if (_missilesRemaining > 0 || _reloadsRemaining < 1) return false;
            _reloadDelay -= Time.deltaTime;
            return _reloadDelay <= 0f;
        }
    }

    void Awake()
    {
        MissileFired = new UnityEvent();
        MissilesReloaded = new UnityEvent();
        _transform = transform;
        _radarScreen = FindObjectOfType<RadarScreen>();
        _audioSource = SoundManager.Configure3DAudioSource(GetComponent<AudioSource>());
        InitializePoolStrategy();
    }

    void InitializePoolStrategy()
    {
        if (!PoolManager.Instance)
        {
            Debug.LogWarning("PoolManager not found. MissileLauncher will use Instantiate fallback");
            return;
        }
        
        var poolRoot = PoolManager.Instance.GetPoolRoot();
        _missileStrategy = new MissilePoolStrategy(
            _missilePrefab, 
            poolRoot,
            _initialPoolSize,
            _maxPoolSize);
    }

    void OnEnable()
    {
        _missilesRemaining = _missiles;
        _reloadsRemaining = _reloads;
        _fireDelay = 0f;
        _reloadDelay = _reloadTime;
    }

    void Update()
    {
        if (CanReload)
        {
            ReloadMissiles();
        }
        if (CanFire && _weaponInput is {SecondaryFired: true})
        {
            FireMissile();
        }
    }

    void FireMissile()
    {
        if (_launchSound) _audioSource.PlayOneShot(_launchSound);
        
        Missile missile = GetMissileFromPool();

        if (!missile)
        {
            Debug.LogWarning("Failed to get missile from pool. Using Instantiate fallback.");
            missile = Instantiate(_missilePrefab, _transform.position, _transform.rotation).GetComponent<Missile>();
        }
        else
        {
            missile.transform.SetPositionAndRotation(_transform.position, _transform.rotation);
        }
        
        if (_radarScreen)
        {
            missile.Init(_target ? _target : _radarScreen.LockedOnTarget);
        }
        
        missile.gameObject.SetActive(true);
        _missilesRemaining--;
        _fireDelay = _coolDown;
        MissileFired.Invoke();
    }

    Missile GetMissileFromPool()
    {
        if (!PoolManager.Instance) return null;
        return PoolManager.Instance.Get(_missileStrategy);
    }

    void ReloadMissiles()
    {
        _reloadsRemaining--;
        _missilesRemaining = _missiles;
        _reloadDelay = _reloadTime;
        _fireDelay = 0f;
        MissilesReloaded.Invoke();
    }

    public void Init(IWeaponControls weaponInput)
    {
        _weaponInput = weaponInput;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}
