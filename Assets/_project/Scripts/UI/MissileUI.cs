using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.SpaceShooter.Events;

public class MissileUI : MonoBehaviour
{
    [SerializeField] Transform[] _missileAmmo;
    [SerializeField] GameObject _missileDisplayPrefab;
    [SerializeField] GameObject _reloadedDisplay;
    [SerializeField] Image _reloadingBar;
    [SerializeField] TMP_Text _reloadsRemaining;

    MissileLauncher[] _missileLaunchers;

    void OnEnable()
    {
        EventBus.Instance.Subscribe<WeaponSystemsInitializedEvent>(OnWeaponSystemsInitialized);
    }

    void OnDisable()
    {
        if (EventBus.Instance)
        {
            EventBus.Instance.Unsubscribe<WeaponSystemsInitializedEvent>(OnWeaponSystemsInitialized);
        }

        UnsubscribeFromLaunchers();
    }

    void OnWeaponSystemsInitialized(WeaponSystemsInitializedEvent e)
    {
        if (!e.IsLocalPlayer) return;

        UnsubscribeFromLaunchers();

        _missileLaunchers = e.MissileLaunchers;

        if (_missileLaunchers == null || _missileLaunchers.Length == 0) return;

        foreach (var launcher in _missileLaunchers)
        {
            if (!launcher) continue;
            launcher.MissileFired.AddListener(UpdateMissileDisplay);
            launcher.MissilesReloaded.AddListener(OnReloadCompleted);
        }

        UpdateMissileDisplay();
        OnReloadCompleted();
    }

    void UnsubscribeFromLaunchers()
    {
        if (_missileLaunchers == null) return;

        foreach (var launcher in _missileLaunchers)
        {
            if (!launcher) continue;
            launcher.MissileFired.RemoveListener(UpdateMissileDisplay);
            launcher.MissilesReloaded.RemoveListener(OnReloadCompleted);
        }
    }

    void LateUpdate()
    {
        if (_missileLaunchers == null || _missileLaunchers.Length == 0) return;
        if (!_missileLaunchers[0]) return;

        if (!_missileLaunchers[0].Reloading)
        {
            if (!_reloadedDisplay || !_reloadedDisplay.activeSelf) return;
            if (_reloadingBar) _reloadingBar.fillAmount = 0;
            _reloadedDisplay.SetActive(false);
            return;
        }

        if (_reloadingBar)
        {
            _reloadingBar.fillAmount = Mathf.Lerp(_reloadingBar.fillAmount,
                _missileLaunchers[0].ReloadPercent, 10f * Time.deltaTime);
        }

        if (_reloadedDisplay && !_reloadedDisplay.activeSelf)
        {
            _reloadedDisplay.SetActive(true);
        }
    }

    void UpdateMissileDisplay()
    {
        if (_missileLaunchers == null || _missileLaunchers.Length == 0) return;

        for (var i = 0; i < _missileLaunchers.Length; ++i)
        {
            if (i >= _missileAmmo.Length) continue;
            if (!_missileAmmo[i])
            {
                Debug.LogWarning($"Missile ammo display transform not assigned for launcher index {i}");
                continue;
            }
            if (!_missileLaunchers[i]) continue;

            while (_missileAmmo[i].childCount < _missileLaunchers[i].MissileCapacity)
            {
                Instantiate(_missileDisplayPrefab, _missileAmmo[i]);
            }

            for (var m = 0; m < _missileAmmo[i].childCount; ++m)
            {
                if (!_missileAmmo[i] || !_missileAmmo[i].GetChild(m))
                {
                    Debug.LogWarning($"Could not find missile display child {m} for launcher index {i}");
                    continue;
                }
                _missileAmmo[i].GetChild(m).gameObject.SetActive(m < _missileLaunchers[i].Missiles);
            }
        }
    }
    
    void OnReloadCompleted()
    {
        if (_missileLaunchers == null || _missileLaunchers.Length == 0) return;

        UpdateMissileDisplay();

        if (!_reloadsRemaining)
        {
            Debug.LogWarning("MissileUI: Reloads remaining text not assigned.");
            return;
        }

        if (_missileLaunchers[0])
        {
            _reloadsRemaining.text = $"Reloads: {_missileLaunchers[0].Reloads}";
        }
    }
}