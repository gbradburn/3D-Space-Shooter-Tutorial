using System;
using UnityEngine;
using UnityEngine.UI;

public class TargetIndicator : MonoBehaviour
{
    [SerializeField] Image _targetBox, _lockOnImage, _offScreenImage;
    [SerializeField] GameObject _targetLeadIndicatorPrefab;
    [SerializeField] float _offScreenMargin = 45f, _projectileSpeed = 800f;

    Transform _target;
    Canvas _mainCanvas;
    Camera _mainCamera;
    RectTransform _rectTransform, _canvasRect;
    
    Rigidbody _targetRigidbody;
    Transform _player;
    GameObject _targetLeadIndicator;

    Vector3 _screenCenter = Vector3.zero;

    Vector3 ScreenCenter
    {
        get
        {
            var rect =  _canvasRect.rect;
            _screenCenter.x = rect.width * 0.5f;
            _screenCenter.y = rect.height * 0.5f;
            _screenCenter.z = 0f;
            return _screenCenter * _canvasRect.localScale.x;
        }
    }
    
    public int Key { get; private set; }
    public bool LockedOn { get; set; }

    public void Init(Transform target, Canvas mainCanvas)
    {
        _target = target;
        Key = _target.GetInstanceID();
        _mainCanvas = mainCanvas;
        _canvasRect = _mainCanvas.GetComponent<RectTransform>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _targetLeadIndicator = Instantiate(_targetLeadIndicatorPrefab, _mainCanvas.transform);
        _targetRigidbody = _target.GetComponent<Rigidbody>();
        _mainCamera = Camera.main;
    }

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    void OnDestroy()
    {
        _targetLeadIndicator.SetActive(false);
        GameObject.Destroy(_targetLeadIndicator);
    }

    void LateUpdate()
    {
        // Get normalized position of target
        Vector3 targetViewportPos = _mainCamera.WorldToViewportPoint(_target.position);
        
        // Display target reticle on target
        if (TargetIsVisible(targetViewportPos))
        {
            DisplayOnScreenReticle(targetViewportPos);
            UpdateTargetLeadingIndicator();
            return;
        }

        _targetLeadIndicator.SetActive(false);
        DisplayOffScreenReticle(targetViewportPos);
    }

    void UpdateTargetLeadingIndicator()
    {
        var predictedPosition = CalculatePredictedPosition();
        DisplayTargetLeadIndicator(predictedPosition);
    }

    Vector3 CalculatePredictedPosition()
    {
        var interceptPosition = _target.position;

        // Relative position and velocity
        var relativePosition = interceptPosition - _player.position;
        var relativeVelocity = _targetRigidbody.linearVelocity;

        // Coefficients for the quadratic equation
        var a = Vector3.Dot(relativeVelocity, relativeVelocity) - _projectileSpeed * _projectileSpeed;
        var b = 2 * Vector3.Dot(relativeVelocity, relativePosition);
        var c = Vector3.Dot(relativePosition, relativePosition);

        // Check for near-zero 'a' to avoid numerical instability
        if (Mathf.Abs(a) < Mathf.Epsilon)
        {
            return interceptPosition; // Return current position if 'a' is effectively zero
        }

        // Calculate the discriminant of the quadratic equation
        var discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            return interceptPosition; // No real solution, return current position
        }

        // Calculate the two possible times to impact
        var sqrtDiscriminant = Mathf.Sqrt(discriminant);
        var time1 = (-b - sqrtDiscriminant) / (2 * a);
        var time2 = (-b + sqrtDiscriminant) / (2 * a);

        // We want the positive time to impact (the future)
        var impactTime = Mathf.Max(time1, time2);
        if (impactTime < Mathf.Epsilon)
        {
            return interceptPosition; // No valid future impact time, return current position
        }

        // Calculate the intercept position
        interceptPosition += _targetRigidbody.linearVelocity * impactTime;

        return interceptPosition;    
    }

    void DisplayTargetLeadIndicator(Vector3 targetPosition)
    {
        var targetViewportPos = _mainCamera.WorldToViewportPoint(targetPosition);
        _targetLeadIndicator.SetActive(TargetIsVisible(targetViewportPos));
        if (!_targetLeadIndicator.activeSelf) return;
        var position = _mainCamera.ViewportToScreenPoint(targetViewportPos);
        position.z = 0;
        _targetLeadIndicator.transform.position = position;        
    }

    bool TargetIsVisible(Vector3 position)
    {
        return (position.x is >= 0 and <= 1 && position.y is >= 0 and <= 1 && position.z >= 0);
    }    
  
    void DisplayOnScreenReticle(Vector3 targetViewportPos)
    {
        Vector3 position = _mainCamera.ViewportToScreenPoint(targetViewportPos);
        position.z = 0;
        _rectTransform.position = position;
        _targetBox.enabled = !LockedOn;
        _lockOnImage.enabled = LockedOn;
        _offScreenImage.enabled = false;
    }
    
    void DisplayOffScreenReticle(Vector3 targetViewportPos)
    {
        _targetBox.enabled = false;
        _lockOnImage.enabled = false;

        Vector3 indicatorPosition = (_mainCamera.ViewportToScreenPoint(targetViewportPos) - ScreenCenter) *
                                    Mathf.Sign(targetViewportPos.z);
        indicatorPosition.z = 0;

        float x = (ScreenCenter.x - _offScreenMargin) / Mathf.Abs(indicatorPosition.x);
        float y = (ScreenCenter.y - _offScreenMargin) / Mathf.Abs(indicatorPosition.y);

        if (x < y)
        {
            float angle = Vector3.SignedAngle(Vector3.right, indicatorPosition, Vector3.forward);
            indicatorPosition.x = Mathf.Sign(indicatorPosition.x) * (_screenCenter.x - _offScreenMargin) *
                                  _canvasRect.localScale.x;
            indicatorPosition.y = Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.x;
        }
        else
        {
            float angle = Vector3.SignedAngle(Vector3.up, indicatorPosition, Vector3.forward);
            indicatorPosition.y = Mathf.Sign(indicatorPosition.y) * (_screenCenter.y - _offScreenMargin) *
                                  _canvasRect.localScale.y;
            indicatorPosition.x = -Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.y;
        }

        indicatorPosition += ScreenCenter;
        _rectTransform.position = indicatorPosition;

        Vector3 rotation = _rectTransform.eulerAngles;
        rotation.z = Vector3.SignedAngle(Vector3.up, indicatorPosition - ScreenCenter, Vector3.forward);
        _rectTransform.eulerAngles = rotation;
        _offScreenImage.enabled = true;

    }


}
