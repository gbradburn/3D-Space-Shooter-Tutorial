using UnityEngine;
using UnityEngine.UI;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.SpaceShooter.Events;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] Image _healthBarFill;
    [SerializeField] Gradient _healthColorGradient;
    
    [Header("Shield UI")]
    [SerializeField] Image _shieldBarFill;
    [SerializeField] Color _shieldColor = new Color(0f, 0.75f, 1f);
    
    [Header("Animation")]
    [SerializeField] float _updateSpeed = 5f;

    DamageHandler _shipDamageHandler;
    DamageHandler _shieldDamageHandler;
    GameObject _trackedPlayer;
    float _targetHealthFill;
    float _targetShieldFill;

    void OnEnable()
    {
        EventBus.Instance.Subscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
        EventBus.Instance.Subscribe<PlayerDestroyedEvent>(OnPlayerDestroyed);
    }

    void OnDisable()
    {
        if (EventBus.Instance)
        {
            EventBus.Instance.Unsubscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
            EventBus.Instance.Unsubscribe<PlayerDestroyedEvent>(OnPlayerDestroyed);
        }

        UnsubscribeFromCurrentPlayer();
    }

    void OnPlayerSpawned(PlayerSpawnedEvent e)
    {
        if (!e.IsLocalPlayer) return;
        
        SetTrackedPlayer(e.Player);
    }

    void OnPlayerDestroyed(PlayerDestroyedEvent e)
    {
        if (_trackedPlayer == e.Player)
        {
            UnsubscribeFromCurrentPlayer();
            _trackedPlayer = null;
        }
    }

    void SetTrackedPlayer(GameObject player)
    {
        UnsubscribeFromCurrentPlayer();
        
        _trackedPlayer = player;
        
        _shipDamageHandler = player.GetComponent<DamageHandler>();
        var shield = player.GetComponentInChildren<Shield>();
        if (shield)
        {
            _shieldDamageHandler = shield.GetComponent<DamageHandler>();
        }
        
        if (_shipDamageHandler)
        {
            _shipDamageHandler.HealthChanged.AddListener(UpdateHealthDisplay);
            UpdateHealthDisplay();
        }

        if (_shieldDamageHandler)
        {
            _shieldDamageHandler.HealthChanged.AddListener(UpdateShieldDisplay);
            _shieldDamageHandler.ObjectDestroyed.AddListener(OnShieldDestroyed);
            UpdateShieldDisplay();
        }
    }

    void UnsubscribeFromCurrentPlayer()
    {
        if (_shipDamageHandler)
        {
            _shipDamageHandler.HealthChanged.RemoveListener(UpdateHealthDisplay);
        }

        if (_shieldDamageHandler)
        {
            _shieldDamageHandler.HealthChanged.RemoveListener(UpdateShieldDisplay);
            _shieldDamageHandler.ObjectDestroyed.RemoveListener(OnShieldDestroyed);
        }
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
        if (!_shipDamageHandler || _shipDamageHandler.MaxHealth <= 0) return;
        
        var healthPercent = (float)_shipDamageHandler.Health / _shipDamageHandler.MaxHealth;
        _targetHealthFill = healthPercent;

        if (_healthColorGradient != null && _healthBarFill)
        {
            _healthBarFill.color = _healthColorGradient.Evaluate(healthPercent);
        }
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

    void OnShieldDestroyed() => _targetShieldFill = 0f;
}
