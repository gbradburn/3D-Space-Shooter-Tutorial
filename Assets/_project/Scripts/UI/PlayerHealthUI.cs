using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] DamageHandler _shipDamageHandler;
    [SerializeField] DamageHandler _shieldDamageHandler;
    
    [Header("Health UI")]
    [SerializeField] Image _healthBarFill;
    [SerializeField] Gradient _healthColorGradient;
    
    [Header("Shield UI")]
    [SerializeField] Image _shieldBarFill;
    [SerializeField] Color _shieldColor = new Color(0f, 0.75f, 1f);
    
    [Header("Animation")]
    [SerializeField] float _updateSpeed = 5f;

    float _targetHealthFill;
    float _targetShieldFill;

    void OnEnable()
    {
        if (_shipDamageHandler)
        {
            _shipDamageHandler.HealthChanged.AddListener(UpdateHealthDisplay);
            UpdateHealthDisplay();
        }

        if (!_shieldDamageHandler) return;
        _shieldDamageHandler.HealthChanged.AddListener(UpdateShieldDisplay);
        _shieldDamageHandler.ObjectDestroyed.AddListener(OnShieldDestroyed);
        UpdateShieldDisplay();
    }

    void OnDisable()
    {
        if (_shipDamageHandler)
        {
            _shipDamageHandler.HealthChanged.RemoveListener(UpdateHealthDisplay);
        }

        if (!_shieldDamageHandler) return;
        _shieldDamageHandler.HealthChanged.RemoveListener(UpdateShieldDisplay);
        _shieldDamageHandler.ObjectDestroyed.RemoveListener(OnShieldDestroyed);
    }

    void LateUpdate()
    {
        if (_healthBarFill)
        {
            _healthBarFill.fillAmount = Mathf.Lerp(_healthBarFill.fillAmount, 
                _targetHealthFill, 
                _updateSpeed * Time.deltaTime);
        }

        if (_shieldBarFill)
        {
            _shieldBarFill.fillAmount = Mathf.Lerp(_shieldBarFill.fillAmount, 
                _targetShieldFill, 
                _updateSpeed * Time.deltaTime);
        }
    }

    void UpdateHealthDisplay()
    {
        if (!_shipDamageHandler) return;
        if (_shipDamageHandler.MaxHealth <= 0) return;
        var healthPercent = (float)_shipDamageHandler.Health / _shipDamageHandler.MaxHealth;
        _targetHealthFill = healthPercent;

        if (_healthColorGradient == null || !_healthBarFill) return;
        _healthBarFill.color = _healthColorGradient.Evaluate(healthPercent);
    }

    void UpdateShieldDisplay()
    {
        if (!_shieldDamageHandler) return;

        var shieldPercent = (float)_shieldDamageHandler.Health / _shieldDamageHandler.MaxHealth;
        _targetShieldFill = shieldPercent;

        if (_shieldBarFill)
        {
            _shieldBarFill.color = _shieldColor;
        }
    }

    void OnShieldDestroyed()
    {
        _targetShieldFill = 0f;
    }
}
