using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissileUI : MonoBehaviour
{
    [SerializeField] MissileLauncher[] _missileLaunchers;
    [SerializeField] Transform[] _missileAmmo;

    [SerializeField] GameObject _missileDisplayPrefab;
    [SerializeField] GameObject _reloadedDisplay;
    [SerializeField] Image _reloadingBar;
    [SerializeField] TMP_Text _reloadsRemaining;

    void OnEnable()
    {
        foreach (var launcher in _missileLaunchers)
        {
            launcher.MissileFired.AddListener(UpdateMissileDisplay);
            launcher.MissilesReloaded.AddListener(OnReloadCompleted);
        }
        UpdateMissileDisplay();
        OnReloadCompleted();
    }

    void OnDisable()
    {
        foreach (var launcher in _missileLaunchers)
        {
            launcher.MissileFired.RemoveListener(UpdateMissileDisplay);
            launcher.MissilesReloaded.RemoveListener(OnReloadCompleted);
        }
    }

    void LateUpdate()
    {
        if (!_missileLaunchers[0].Reloading)
        {
            if (!_reloadedDisplay.activeSelf) return;
            _reloadingBar.fillAmount = 0;
            _reloadedDisplay.SetActive(false);
            return;
        }

        _reloadingBar.fillAmount = Mathf.Lerp(_reloadingBar.fillAmount,
            _missileLaunchers[0].ReloadPercent, 10f * Time.deltaTime);

        if (!_reloadedDisplay.activeSelf)
        {
            _reloadedDisplay.SetActive(true);
        }

    }

    void UpdateMissileDisplay()
    {
        for (int i = 0; i < _missileAmmo.Length; ++i)
        {
            while (_missileAmmo[i].childCount < _missileLaunchers[i].MissileCapacity)
            {
                Instantiate(_missileDisplayPrefab, _missileAmmo[i]);
            }

            for (int m = 0; m < _missileAmmo[i].childCount; ++m)
            {
                _missileAmmo[i].GetChild(m).gameObject.SetActive(m < _missileLaunchers[i].Missiles);
            }
        }
    }
    
    void OnReloadCompleted()
    {
        UpdateMissileDisplay();
        _reloadsRemaining.text = $"Reloads: {_missileLaunchers[0].Reloads}";
    }

}