using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Image _healthBarImage;
    [SerializeField] float _updateRate = 1f;
    [SerializeField] DamageHandler _damageHandler;

    Transform _transform;
    Camera _camera;
    float _targetFillAmount;

    void Awake()
    {
        _transform = transform;
        _camera = Camera.main;
        if (_damageHandler == null) return;
        UpdateHealthBar();
    }

    void OnEnable()
    {
        if (_damageHandler == null) return;
        _damageHandler.HealthChanged.AddListener(UpdateHealthBar);
        _damageHandler.ObjectDestroyed.AddListener(DisableHealthBar);
    }



    void OnDisable()
    {
        if (_damageHandler == null) return;
        _damageHandler.HealthChanged.RemoveListener(UpdateHealthBar);
        _damageHandler.ObjectDestroyed.RemoveListener(DisableHealthBar);
    }

    void LateUpdate()
    {
        _transform.LookAt(_camera.transform);
        if (_damageHandler == null || _healthBarImage == null) return;
        if (Mathf.Approximately(_healthBarImage.fillAmount, _targetFillAmount)) return;
        _healthBarImage.fillAmount =
            Mathf.MoveTowards(_healthBarImage.fillAmount, _targetFillAmount, _updateRate * Time.deltaTime);
    }

    void UpdateHealthBar()
    {
        if (_damageHandler == null) return;
        if (_damageHandler.Health == 0)
        {
            _targetFillAmount = 0;
            return;
        }

        _targetFillAmount = (float)_damageHandler.Health / (float)_damageHandler.MaxHealth;
    }
    void DisableHealthBar()
    {
        gameObject.SetActive(false);
    }
    
}
