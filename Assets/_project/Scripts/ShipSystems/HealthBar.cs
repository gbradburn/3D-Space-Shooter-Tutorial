using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Image _healthBarImage;
    [SerializeField] float _updateRate = 1f;
    [SerializeField] DamageHandler _damageHandler;

    CameraManager _cameraManager;
    Transform _transform;
    Transform _activeCamera;
    float _targetFillAmount;

    void Awake()
    {
        _transform = transform;
        _cameraManager = FindObjectOfType<CameraManager>();
    }

    void OnEnable()
    {
        if (_damageHandler)
        {
            _damageHandler.HealthChanged.AddListener(UpdateHealthBar);
            _damageHandler.ObjectDestroyed.AddListener(DisableHealthBar);
            _targetFillAmount = 1;
        }

        _activeCamera = _cameraManager.ActiveCamera;
        _cameraManager.ActiveCameraChanged.AddListener(OnCameraChanged);
    }

    void OnDisable()
    {
        if (_damageHandler)
        {
            _damageHandler.HealthChanged.RemoveListener(UpdateHealthBar);
            _damageHandler.ObjectDestroyed.RemoveListener(DisableHealthBar);
        }
        _cameraManager.ActiveCameraChanged.RemoveListener(OnCameraChanged);
    }

    void LateUpdate()
    {
        if (_activeCamera)
        {
            _transform.LookAt(_activeCamera);
        }
        if (!_damageHandler || _healthBarImage == null) return;
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

    void OnCameraChanged()
    {
        _activeCamera = _cameraManager.ActiveCamera;
    }

    
}
