using System;
using System.Timers;
using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    [SerializeField] GameObject _missilePrefab;
    [SerializeField] int _missiles = 4, _reloads = 2;
    [SerializeField] float _coolDown = 2f, _reloadTime = 60f;

    Transform _transform;
    RadarScreen _radarScreen;
    int _missilesRemaining, _reloadsRemaining;
    float _fireDelay, _reloadDelay;

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
        _transform = transform;
        _radarScreen = FindObjectOfType<RadarScreen>();
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

        if (CanFire && Input.GetMouseButtonDown(1))
        {
            FireMissile();
        }
    }

    void FireMissile()
    {
        var missile = Instantiate(_missilePrefab, _transform.position, _transform.rotation).GetComponent<Missile>();
        if (_radarScreen)
        {
            missile.Init(_radarScreen.LockedOnTarget);
        }
        
        missile.gameObject.SetActive(true);
        _missilesRemaining--;
        _fireDelay = _coolDown;
    }

    void ReloadMissiles()
    {
        _reloadsRemaining--;
        _missilesRemaining = _missiles;
        _reloadDelay = _reloadTime;
    }
}
