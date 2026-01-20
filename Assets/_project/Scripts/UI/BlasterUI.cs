using UnityEngine;
using UnityEngine.UI;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.SpaceShooter.Events;

public class BlasterUI : MonoBehaviour
{
    [SerializeField] Image[] _blasterBars, _coolDownBars;

    Blaster[] _blasters;

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
    }

    void OnWeaponSystemsInitialized(WeaponSystemsInitializedEvent e)
    {
        if (!e.IsLocalPlayer) return;
        _blasters = e.Blasters;
    }

    void LateUpdate()
    {
        if (_blasters == null || _blasters.Length == 0) return;

        for (var i = 0; i < _blasters.Length; ++i)
        {
            if (i >= _blasterBars.Length || i >= _coolDownBars.Length) continue;
            if (!_blasters[i]) continue;

            _blasterBars[i].fillAmount = Mathf.Lerp(_blasterBars[i].fillAmount, 
                _blasters[i].CapacitorChargePercentage,
                10f * Time.deltaTime);
            _coolDownBars[i].fillAmount = Mathf.Lerp(_coolDownBars[i].fillAmount,
                _blasters[i].CoolDownPercent, 
                10 * Time.deltaTime);
        }
    }
}
